# DataEncrypter
*DataEncrypter* is an easy to use tool for encryption and decryption. It is build to be lightweight and not memory intensive. It currently has a custom implementation of the Advanced Encryption Standard, but is open to be used with different algorithms as well, if they implement the ICypher interface.

File encryption by using the SecureFile class is very simple to handle. Files are saved as .secf (SECureFile) and provide a encrypted header with previous filename and extension, to restore the file completely after decryption.

Basic usage of SecureFile:
```C#
  var secureFile = new SecureFile(path, key);

  //Handles Memory and feeds the algorithms the correct data
  secureFile.Encrypt();
  secureFile.Decrypt();

  secureFile.Save(newPath); //saves the file
```
