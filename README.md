# Clone Updates of Tiddly Wiki

[Video Demonstration](https://youtu.be/58t4TMxxg_w)

## Project Overview

This project was developed to facilitate the maintenance of Tiddly Wikis in organizational environments, particularly where access to updates is restricted to preserve data integrity and security. The project serves a similar purpose to that of a Database Management System but offers the flexibility of a wiki, allowing for a more dynamic approach to data management. Tiddly Wikis, being open-source single-page applications, are inherently user-friendly and versatile.

In this specific implementation, two Tiddly Wikis were utilized: one main wiki, accessible only to a select group with editing rights, and a source wiki, open to all project participants for departmental updates. After data entry in the source wiki, higher authorities regularly reviewed the data to ensure its integrity before updating the main wiki. Performing this task manually would be time-consuming, as each entry required careful validation. Therefore, the need arose for an automated solution—a program capable of transferring finalized updates from the source wiki to the main wiki with a single click. This project addresses that need.

## How It Works

The core functionality of this project is implemented in a C# script hosted within an Azure Function App, which is triggered via an HTTP request. A button embedded in the main wiki initiates this HTTP request, thereby activating the Function App. During the cloning process, the storage associated with the web application hosting the source wiki is carefully examined for any updates or changes. These updates are then duplicated to the storage of the web application hosting the main wiki.

To achieve this functionality, several critical components must be managed, including environment variables, Azure Active Directory (AD) app registration, and Cross-Origin Resource Sharing (CORS) settings for the Function App.

## Tools and Technologies

1. **TiddlyWiki**: An open-source notebook tool that operates like a wiki, with hierarchical embedding of "Tiddlers." It serves as the foundational platform for this project.
2. **Azure Web App**: The platform that hosts both the source and destination TiddlyWikis.
3. **Azure Storage Account**: A repository for storing and protecting the files essential to the wiki's Tiddlers, including `.tid` files and `$config` files, which contain content and configuration data, respectively.
4. **Azure Function App**: This hosts the C# script responsible for synchronizing updates from the source TiddlyWiki to the destination.
5. **DeleteAsync API**: This API is used to remove `.tid` files from the Azure Storage Account, enabling the duplication of updates.
6. **StartCopy API**: Facilitates the replication of updates by copying `.tid` files from one Azure Storage Account to another.
7. **Restart API**: After updates are replicated, this API restarts the destination Azure Web App, allowing users of the read-only wiki to view updates by simply refreshing the page. A manual restart is also possible if the user has access to the Web App Overview.

## Environment Setup

To set up the environment, follow these steps:

1. **Configure Web Apps**: Set up two Azure Web Apps for the public (read-only) and private (editable) Tiddly Wikis. Use file shares to store necessary files through "Path mappings" in the "Configuration" blade.
2. **Azure AD Configuration**: Set up Azure AD for the public Tiddly Wiki to manage authentication and authorization, as the Azure Function App will need to restart the public (read-only) Tiddly Wiki.
3. **Access Control**: Add a role in the "Access Control (IAM)" blade for the Azure AD App Registration under the Azure subscription to manage authentication and authorization.
4. **Deploy Azure Function App**: Set up an Azure Function App that runs the HTTP-triggered Azure Function, deployed via the workflow in this GitHub repository.
5. **Configure Environment Variables**: Set the necessary environment variables in the Function App's configuration.
6. **CORS Handling**: For the Function App, configure CORS by setting the domain of the Web App that triggers the function as an "Allowed Origin."
7. **Custom Widget Configuration**: In the custom widget created for triggering the HTTP Azure Function, configure the URL with `fetch` to point to `<FunctionAppURL>/api/CopyTWUpdates` and use the function app key for authentication during the fetch call.

## Environment Variables

1. **SUBSCRIPTION_ID**: The subscription ID of the Azure portal.
2. **STORAGE_CONNECTION_STRING**: The connection string of the storage container holding the source and destination file shares.
3. **SOURCE_SHARE_NAME**: The name of the file share for the source.
4. **DESTINATION_SHARE_NAME**: The name of the file share for the destination.
5. **RESOURCE_GROUP**: The resource group containing the App Service for the public (read-only) Tiddly Wiki.
6. **APP_NAME_RESTART**: The name of the Web App hosting the public (read-only) Tiddly Wiki.
7. **CLIENT_SECRET_RESTART**: The client secret of the Azure AD App Registration for the public Tiddly Wiki's Web App.
8. **CLIENT_ID_RESTART**: The client ID of the Azure AD App Registration for the public Tiddly Wiki's Web App.
9. **TENANT_ID**: The tenant ID of the Azure AD App Registration for the public Tiddly Wiki's Web App.
