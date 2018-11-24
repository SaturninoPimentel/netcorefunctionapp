#!/bin/bash

clear
echo "***********************************************************"
echo "******Welcome to azure Configuration Tool******"
echo "***********************************************************"

ConfigureAzureSubscription()
{
    # authenticate to azure suscription
    az login
  
    echo "Verify your default subscription where you created the project resources:"
    az account list 

    read -p "Paste the id of the subscription where you created the project resources: " subscriptionId
    az account set --subscription $subscriptionId

    echo "The subscription you have selected is: "
    az account show

    # all to lower case
    answer=$(echo $answer | awk '{print tolower($0)}')

    ProceedConfiguration
}

ProceedConfiguration ()
{
    read -p "Introduce the name of your project Resource Group: " resourceGroupName

    az group create \
    --location southcentralus \
    --name $resourceGroupName

    ConfigureDatabase

    ConfigureStorage

    ConfigureFunctionApp

    echo "Great, your backend has been configured successfully!"
    #more code here
    exit 0
}

ConfigureStorage()
{
    storageAccountName=$resourceGroupName"storage"
    echo "Storage account name: " $storageAccountName

    az storage account create --name $storageAccountName \
    --location southcentralus \
    --resource-group $resourceGroupName \
    --sku Standard_LRS
    echo "Storage created!"
}

ConfigureDatabase ()
{
    echo "* Configuring: Database>>"

    collectionName="ContactInfoRecords"
    echo "Collection name: " $collectionName
    
    databaseAccountName=$resourceGroupName"-cos"
    echo "Account name: " $databaseAccountName
    
    databaseName="ContactInfoDB"
    echo "Database name: " $databaseName
   
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
    echo "Account created!"

    # create database 
    az cosmosdb database create \
    --name $databaseAccountName \
    --db-name $databaseName \
    --resource-group $resourceGroupName
    echo "Database created!"

    # create collection in documentDB database
    az cosmosdb collection create \
	--collection-name $collectionName \
	--name $databaseAccountName \
	--db-name $databaseName \
	--resource-group $resourceGroupName    
    echo "Database configured successfully!"
}

ConfigureFunctionApp ()
{
    echo "* Configuring: Function App>>"

    functionAppName=$resourceGroupName"-func"
    echo "Function App name: " $functionAppName
    
    # create azure function
    az functionapp create \
    --resource-group $resourceGroupName \
    --consumption-plan-location westus \
    --name $functionAppName \
    --os-type Linux \
    --runtime dotnet \
    --storage-account $storageAccountName
    echo "function app created!"

    # configure database uri
    az functionapp config appsettings set --resource-group $resourceGroupName --name $functionAppName --settings COSMOSDB_CONNECTIONSTRING=$databaseUri

    # configure database id
    az functionapp config appsettings set --resource-group $resourceGroupName --name $functionAppName --settings CosmosDB_DatabaseId=ContactInfoDB

    # configure database person collection
    az functionapp config appsettings set --resource-group $resourceGroupName --name $functionAppName --settings COSMOSDB_CONTACTINFOCOLLECTION=ContactInfoRecords
    
    echo "Function App configured successfully!"
}

ConfigureAzureSubscription