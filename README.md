# Generate Request and Certificate PKI Application

Primary this application was develop for creating certificate request for web server with additional subject names.
Generated certificate file with .csr extension need to be send to internal or external certificate authorities (CA) to generate certificate file. This generated certificate file will be connecting with certificate private key to generate .pfx certificate file for web server.
Certificate file generated on this way with internal or external CA public key certificate will be used on web server to provide web server authentication.

After initial version, I added new option for generating self-sign certificate that can be generate root CA certificate and other option for generating certificate base on date inside certificate request file (.csr).

This application can be used for practical and educational purpose.
	
This is Windows Desktop application developed in C#, using WPF MahApps and Bouncy Castle PKI libraries.

## How to use

You can start application and choose between 4 menu options. For complete help read user manual.

## User manual
	
Complete help with screenshots you can find inside "GenCertHelp" folder in different file formats.
I used chmProcessor tool from location http://chmprocessor.sourceforge.net/ to generate different file formats from MS Word document.

To generate this type of help documentations you need to download software and make chmProcessor project file (file with .WHC extension inside "GenCertHelp" folder).

## How to successfully build application from source code

If you deleted or changed help documentation inside GenCertHelp folder, must build help documentation inside that folder.
To successfully build documentation use chmProcessor tool from location http://chmprocessor.sourceforge.net/.
Then rebuild Visual Studio project.
All complete binary distribution you will found inside out folder of your Visual Studio project directory.
