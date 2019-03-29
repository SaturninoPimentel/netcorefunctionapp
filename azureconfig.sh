#!/bin/bash

clear
echo "***********************************************************"
echo "******Welcome to azure Configuration Tool******"
echo "***********************************************************"

ConfigureAzureSubscription()
{
    # authenticate to azure suscription
    az login
  
    echo "Listado de suscripciones, por favor seleccione donde desea crear el grupo de recursos"
    az account list 

    read -p "Escriba el id de la suscripción donde creará el grupo de recursos: " subscriptionId
    az account set --subscription $subscriptionId

    echo "La suscripción que ha seleccionado es: "
    az account show

    # all to lower case
    answer=$(echo $answer | awk '{print tolower($0)}')

    ProceedConfiguration
}

ProceedConfiguration ()
{
    read -p "Ingresa el nombre de tu grupo de recursos: " resourceGroupName

    az group create \
    --location southcentralus \
    --name $resourceGroupName

    ConfigureDatabase

    ConfigureStorage

    ConfigureFunctionApp

    echo "Excelente, tu backend se ha configurado de forma éxitosa!"
    #more code here
    exit 0
}

ConfigureStorage()
{
    storageAccountName=$resourceGroupName"storage"
    echo "Nombre de storage account: " $storageAccountName

    az storage account create --name $storageAccountName \
    --location southcentralus \
    --resource-group $resourceGroupName \
    --sku Standard_LRS
    echo "Storage creado!"
}

ConfigureDatabase ()
{
    echo "* Configurando: Database>>"

    collectionName="ContactInfoRecords"
    echo "Nombre de Collection: " $collectionName
    
    databaseAccountName=$resourceGroupName"-cos"
    echo "Nombre de cuenta: " $databaseAccountName
    
    databaseName="ContactInfoDB"
    echo "Nombre de database: " $databaseName
   
   # Create a MongoDB API Cosmos DB account
    az cosmosdb create \
	--name $databaseAccountName \
	--kind MongoDB \
	--locations "South Central US"=0 \
	--resource-group $resourceGroupName \
	--max-interval 10 \
	--max-staleness-prefix 200

    cosmosdbId=$(az cosmosdb show --name ${databaseAccountName} --resource-group ${resourceGroupName} \
    --output tsv --query id)

    databaseUri=$(az cosmosdb list-connection-strings --ids $cosmosdbId \
    --query "connectionStrings[0].connectionString" --output tsv)

    echo "Cadena de conexión mongodb"
    echo $databaseUri
    echo "cuenta creada!"

    # create database 
    az cosmosdb database create \
    --name $databaseAccountName \
    --db-name $databaseName \
    --resource-group $resourceGroupName
    echo "Base de datos creada!"

    # create collection in documentDB database
    az cosmosdb collection create \
	--collection-name $collectionName \
	--name $databaseAccountName \
	--db-name $databaseName \
	--resource-group $resourceGroupName    
    echo "Base de datos configurada éxitosamente!"
}

ConfigureFunctionApp ()
{
    echo "* Configurando: Function App>>"

    functionAppName=$resourceGroupName"-func"
    echo "Nombre de function App: " $functionAppName
    
    # create azure function
    az functionapp create \
    --resource-group $resourceGroupName \
    --consumption-plan-location westus \
    --name $functionAppName \
    --os-type Windows \
    --runtime dotnet \
    --storage-account $storageAccountName
    echo "function app creada!"

    # configure database uri
    az functionapp config appsettings set --resource-group $resourceGroupName --name $functionAppName --settings COSMOSDB_CONNECTIONSTRING=$databaseUri

    # configure database id
    az functionapp config appsettings set --resource-group $resourceGroupName --name $functionAppName --settings CosmosDB_DatabaseId=ContactInfoDB

    # configure database person collection
    az functionapp config appsettings set --resource-group $resourceGroupName --name $functionAppName --settings COSMOSDB_CONTACTINFOCOLLECTION=ContactInfoRecords
    
    echo "Function App configurada éxitosamente!"
}

ConfigureAzureSubscription