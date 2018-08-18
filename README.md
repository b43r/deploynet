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

## Command reference

#### alert
```xml
<alert msg="This is a demo of all commands." title="Important message" />
```

#### build
```xml
<build project="MyProject.csproj" platform="x86" configuration="Release" output="d:\temp\build_output" target="" />
```
Build a Visual Studio project.

#### copy
```xml
  <!-- copy a file or folder (including all files and subfolders), 'dst' may include placeholders -->
  <copy src="d:\file1.txt" dst="d:\file2.txt" />
  <copy src="d:\file1.txt" dst="d:\file-{DATE:yyyy-MM-dd}.txt" />
  <copy src="d:\folder1" dst="d:\folder2" />
```
Copy a file or folder (including all files and subfolders). 'dst' may contain a placeholder {DATE} for the current date and time.

#### delay
```xml
<delay ms="1000" />
```
Delay the execution of the script by some milliseconds.

#### delete
```xml
<delete file="d:\file1.txt" />
<delete folder="d:\folder1" recursive="true" />
```
Delete a file or folder. Note that deleting a non-empty folder will fail unless recursive="true" is specified.

#### error
```xml
<error>Message</error>
```
Display an error message and abort the script.

#### ftp
```xml
<ftp server="ftp.server.com" user="anonymous" password="anon@mail.com">
  <upload src="d:\file1.txt" dst="file1.txt" />
  <download src="file2.txt" dst="d:\file2.txt" />
</ftp>
```
Upload or download files to/from an FTP server.

#### ifexists
```xml
<ifexists file="d:\file1.txt">
  <alert msg="File exists!" />
</ifexists>
<ifexits folder="d:\folder1">
  <alert msg="Folder exists!" />
</ifexits>
```
Executes other commands only if a file or folder exists. Any number of commands can be nested in the ```<ifexists></ifexists>``` tag.

#### ifnotexists
```xml
<ifnotexists file="d:\file2.txt"></ifnotexists>
<ifnotexists folder="d:\folder2"></ifnotexists>
```
Executes other commands only if a file or folder NOT exists.

#### iisapppool
```xml
<iisapppool name="Test" action="stop"/>
<iisapppool name="Test" action="start"/>
<iisapppool name="Test" action="start"/>
```
Start, stop or recylce an IIS application pool.

#### iisapppool
```xml
<move src="d:\file1.txt" dst="d:\folder2\subfolder\file1.txt" />
<move src="d:\folder1" dst="d:\folder2\subfolder\folder1" />
```
Move a file or folder, 'dst' may contain a placeholder {DATE} for the current date and time.

#### rename
```xml
<rename src="d:\file1.txt" dst="file3.txt" />
<rename src="d:\folder1" dst="folder3" />
```
Rename a file or folder, 'dst' may contain a placeholder {DATE} for the current date and time.

#### replacetext
```xml
<replacetext src="string BuildDate = &quot;.*&quot;" dst="BuildDate: {DATE}" file="d:\version.cs" />
```
Replace text in a file, 'dst' may contain a placeholder {DATE} for the current date and time.

#### run
```xml
<run file="notepad.exe" arguments="license.txt" />
```
Run an external program.

#### service
```xml
<service name="MyServiceName" action="start" />
<service name="MyServiceName" action="stop" />
```
Start or stop a Windows service.

#### unzip
```xml
<unzip src="d:\MyApp.zip" dst="d:\MyApp" check="true" />
```
Unzip an archive file into a directory. If the extension of the archive is 7z, 7zip will be used. If "check" is set to "true", the archive integrity will be checked prior to extracting (7zip only).

#### zip
```xml
<zip src="d:\temp\build_output" dst="d:\MyApp.zip" fileMask="-.pdb;-.xml" recursive="true" />    
```
Zip files in a directory into an archive. 'dst' may contain a placeholder {DATE} for the current date and time. "fileMask" may contain multiple regular expressions (separated by ;) which are checked against the name of the source files. If an expression starts with a "-", files matching the regex are *excluded* from the archive. If the extension of the archive is 7z, 7zip will be used.
