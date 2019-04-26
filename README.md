# Decrypt Tool

* This is a personal decryption tool, easy to use with user-friendly interface,
* It is used to decrypt the encrypted text resulted from using the MultiEncrypt tool.


## Decrypt interface

This is how the Decrypt tool looks like for the user.

![alt text](https://github.com/andrei-voia/decrypt_tool/blob/master/Screenshot_7.png "looks")


# How to use

* The way you use this program is pretty straightforward,
* Here i will try to present and explain the methods you can use to decrypt a text exactly how you want:

## No-Key situation

* In case you encrypted a text without using a Key, then there is a randomized Key saved in the encrypted text and this Decrypt algorithm is able to detect it and decrypt the text without using any Key.

![alt text](https://github.com/andrei-voia/decrypt_tool/blob/master/Screenshot_3.png "looks")


## With Key situation

* In case you used a Key for encryption, then you will have to use a key to decrypt the message
* Here are some examples of a message encrypted using the "rada" Key:


Working case:

![alt text](https://github.com/andrei-voia/decrypt_tool/blob/master/Screenshot_5.png "looks")


Not inserting any Key when the text was encrypted with a Key:

![alt text](https://github.com/andrei-voia/decrypt_tool/blob/master/Screenshot_4.png "looks")


Decrypting with a wrong Key:

![alt text](https://github.com/andrei-voia/decrypt_tool/blob/master/Screenshot_8.png "looks")


## More functionalities

You can either read a text from a .txt file and write the decrypted text in a .txt folder to simplify the process of saving and loading texts.

This is an example of saving to file method:

![alt text](https://github.com/andrei-voia/multi_encrypt_tool/blob/master/Screenshot_6.png "looks")


This is an example of reading from file method:

![alt text](https://github.com/andrei-voia/multi_encrypt_tool/blob/master/Screenshot_2.png "looks")


## Bundle programs

The MultiEncrypt Tool should be used together with the Decrypt Tool to succesfully extract the hidden message from the encrypted text.

![alt text](https://github.com/andrei-voia/multi_encrypt_tool/blob/master/Screenshot_1.png "looks")
