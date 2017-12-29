# RemoteAppServiceExample
Example app of hosting an AppService on a remote machine.

#Things to remember:
Provider project:
	Package.appxmanifest:
		tab Capabilities:
			Add capability "remoteSystem"
			Add capability "privateNetworkClientServer"
		tab Declarations:
			Add App Service declaration
				set Name and Entrypoint (points to service project, class that inherits IBackgroundTask)
		
		extra changes needed in xml!
			change <uap:AppService Name="com.poctools.remoteservice" />
			to <uap3:AppService Name="com.poctools.remoteservice" SupportsRemoteSystems="true" /> note uap3 namespace!
	
	Add reference to Service project!

Service project is a Windows Component project type.

Client project
	Package.appxmanifest:
		tab Capabilities:
			Add capability "remoteSystem"
			Add capability "privateNetworkClientServer"
