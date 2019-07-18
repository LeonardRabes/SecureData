# DataEncrypter
*DataEncrypter* is an easy to use tool for encryption and decryption. It is build to be lightweight and not memory intensive. It currently has a custom implementation of the Advanced Encryption Standard, but is open to be used with different algorithms as well, if they implement the ICypher interface.

## Installation
Download the latest release of DataEncrypter and save it. This program uses SDelete for file removal (Download: https://docs.microsoft.com/en-us/sysinternals/downloads/sdelete) to securely delete unwanted files. Please drop the *sdelet.exe* into the same directory as the *DataEncrypter.exe*. Thats it you can now encrypt and decrypt files.

## Usage of SecureFile
File encryption by using the SecureFile class is very simple to handle. Files are saved as .secf (SECureFile) and provide a encrypted header with previous filename and extension, to restore the file completely after decryption.

Basic usage of SecureFile:
```C#
  var secureFile = new SecureFile(key);

  //Handles Memory and feeds the algorithms the correct data, saves file, can delete original file
  secureFile.Encrypt(path, saveDirectory, deleteOriginal);
  secureFile.Decrypt(path, saveDirectory, deleteOriginal);
```
