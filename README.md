# Deploy.NET
Script runner for deploying applications to production with a single click.

Deploy.NET is typically installed on your production or test server and runs a deployment script with a single click. A typical deployment task may look like this:


- Stop a local Windows service and IIS application pool
- Create a ZIP file of the currently installed Windows service and web application and copy it into a backup folder
- Download ZIP files containing your lates build from your FTP server
- Unzip the downloaded files and overwrite the installed Windows service and web application
- Start the Windows service and IIS application pool

## Getting started

After installing Deploy.NET, all script files with the extension *.deploy* are automatically associated with Deploy.NET and can be run by doubleclicking them. So let's create your first deploy script!

Create a new file using Notepad or any other text editor, copy the following XML text into it, and save it under the name *demo.deploy*.

```xml
<?xml version="1.0" encoding="utf-8" ?>
<deployNET version="1.0" requiresAdmin="false">

  <ftp server="ftp.server.com" user="anonymous" password="anon@mail.com">
    <download src="build.zip" dst="c:\temp\build.zip" />
  </ftp>
  
  <unzip src="c:\temp\build.zip" dst="c:\inetpub\wwwroot\MyApp" />
  
</deployNET>
```

Now you should be able to doubleclick your *demo.deploy* file to automatically start Deploy.NET. Don't worry, the script will not run before you click the "Start" button!
![Deploy.NET UI](/deploynet1.png)

If you run the script, it will download a file called *build.zip* from an FTP server into the temp directory. Then the ZIP file will be extracted to *c:\inetpub\wwwroot\MyApp*.


