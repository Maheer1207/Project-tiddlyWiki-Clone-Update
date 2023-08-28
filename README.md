# Clone and Update Tiddly Wiki

This project was developed for an organization with the aim of maintaining Tiddly Wikis in scenarios like the Database Management System of an organization. In situations where not everyone has access to updates due to limitations that help preserve data integrity and security, this project serves a similar purpose. However, it takes the form of a wiki, offering significantly more flexibility in terms of the data we can add and how we can manage it. Moreover, Tiddly Wikis are open-source single-page applications, which makes them user-friendly and easy to use.

In my case, I had two Tiddly Wikis. One of them was the main wiki, accessible only to a limited number of individuals with editing rights. This version was presented to clients. The other wiki (source wiki) was open to all individuals working on the project, enabling them to input updates specific to their respective departments. Following data entry, the higher authorities conducted regular data checks to ensure data integrity and update the main Tiddly Wiki. Performing this task manually would have been time-consuming, as each entry needed to be accurate and input one by one. Therefore, they sought a solution—a program that could, with a simple click of a button, transfer all the finalized updates from the second wiki to the main wiki. This is where the project comes into play.

## How it works

The C# project responsible for executing the entire process is hosted within an Azure Function App, triggered via HTTP. Within the main wiki, a button is situated to initiate the HTTP request, thereby launching the Function App. Throughout the cloning process, the storage of the Web App that hosts the source wiki is meticulously examined for any updates or changes that might have been applied. Following this examination, all the updates and modifications are then duplicated to the storage of the Web App responsible for hosting the main wiki.

To successfully accomplish this task, a few essential elements require management: environment variables, Azure AD app registration, and the CORS settings of the Function App.

## Tools Implemented or Playing a Key Role:

1. TiddlyWiki Tool: An open-source notebook tool, TiddlyWiki operates much like a wiki, with Tiddlers embedded hierarchically. Within projects, it provides the fundamental platform on which the project is developed.
2. Azure Web App: This platform hosts both the source and destination TiddlyWikis.
3. Azure Storage Account: Serving as a repository, the Azure Storage Account stores and safeguards the files essential for the wiki's Tiddlers. These include ".tid" files and "$config" files, which respectively hold the content and configurations of the Tiddlers.
4. Azure Function App: Housed here is the C# script responsible for updating the destination based on the source TiddlyWiki.
5. DeleteAsync API: This API removes ".tid" files from the Azure Storage Account to facilitate the duplication of updates.
6. StartCopy API: Enabling the replication of updates, this API duplicates ".tid" files from one Azure Storage Account to another.
7. Restart API: Following the replication of updates, this API restarts the destination Azure Web App. Consequently, users of the destination (read-only) can view updates by simply refreshing the page. A manual Web App restart is an option if the user has access to the Web App Overview.
   
## Steps to setup the environment

1. Set two Web Apps for the public (read-only) and private (editable) Tiddly Wikis, with file shares to the store files required for the Tiddly Wikis through "Path mappings" in the "Configuration" blade.
2. Set the Azure AD for the public Tiddly Wiki to do authentication and authorization since we want to restart the public (read-only) Tiddly Wiki through the Azure Function App.
3. Add a role in the "Access Control (IAM)" blade for the Azure AD App Registation under our Azure subscription for authentication and authorization processes.
4. Set up an Azure Function App that runs the HTTP Trigger Azure Function and is deployed through this GitHub repo's workflow
5. Set the Environment Variable in the Function App's Configuration.
6. For the Function App, to Handle CORS, set the domain of the Web App that will be triggering the function as "Allowed Origins."
7. In the Custom Widget created for triggering the HTTP Azure Function, set the URL as "fetch" as "<FunctionAppURL>/api/CopyTWUpdates" and for the call, use the function app APP key while fetching.

## Environment Variables

1. SUBSCRIPTION_ID: The "ID" we have for our Azure portal's subscription.
2. STORAGE_CONNECTION_STRING: The "CONNECTION STRING" of the storage container that holds the files source and destination file shares.
3. SOURCE_SHARE_NAME: "FILESHARE NAME" of the source.
4. DESTINATION_SHARE_NAME: "FILESHARE NAME" of the destination.
5. RESOURCE_GROUP: "RESOURCE GROUP" holds the App Service for the Tiddly Wiki we want to restart, the public (read-only) one.
6. APP_NAME_RESTART: "WEB APP" name that hosts the public (read-only) Tiddly Wiki
7. CLIENT_SECRET_RESTART: The "CLIENT SECRET" of the Azure AD App Regisration for the public Tiddly Wiki's Web App
8. CLIENT_ID_RESTART: The "CLIENT ID" of the Azure AD App Regisration for the public Tiddly Wiki's Web App.
9. TENANT_ID: The "TENANT_ID" of the Azure AD App Regisration for the public Tiddly Wiki's Web App.
