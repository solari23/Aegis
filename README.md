**NOTE: Aegis is an incomplete work-in-progress.**

# Aegis
A utility to create and manage encrypted archives for sensitive files.

## What is it?
Like the tagline says, Aegis is a lightweight command-line utility that can create and manage encrypted file archives in a custom file format. The encrypted archive can safely be stored in one's favourite cloud storage service without worrying about leaks.

The design allows for:
- Different types of user keys
- Multiple user keys that can be used to decrypt the data
- Decrypting/accessing one file without having to decrypt them all
- Easy extensibility to add new crypto/key derivation algorithms
- Swapping frontends, since the core logic is implemented as a library


## What's left to implement?
The current version of the app implements all the basic functionality to securely archive files from a command-line interface, including a basic REPL. Currently only passwords are supported as keys, but you can have more than one for a given archive.

Long term vision is:
- Introduce user keys that aren't passwords. Ideally I'd like to implement keys derived from Fido 2.0 hardware.
- Add a GUI. I've never been a fan of writing UI code, and I'm no UX designer so this might never happen.
- Add support for better KDFs than PBKDF2 (e.g. scrypt).
- Support changing/upgrading archive security. Chances are this will require dumping existing user keys and re-encrypting the archive. 


## QNA (Questions Nobody Asked)
#### Q: Why not just use `$ExistingSolution`?
I *did* (half-heartedly) look at a few existing solutions but, to be honest, it just seemed like a fun side project ¯\\_(ツ)_/¯

#### Q: Are you a crypto/security expert?
[lol](https://i.kym-cdn.com/entries/icons/original/000/008/342/ihave.jpg).

#### Q: If I use Aegis, can a bad guy pwn my files?
Maybe.

#### Q: Will you ever finish Aegis?
I'd like to use it myself. It's a side project though, so no promises. I'll keep working on it in spare time/as long as it's still fun.

#### Q: What do you plan to use it for?
The idea popped up while I was working on filing taxes. I currently store those kinds of documents securely but without good redundancy. What I wanted was a lightweight but flexible solution to plop those files into cloud storage and not have to worry about them.

#### Q: Be honest. Did you just set up the project as an excuse to play with the preview versions of VS2019/C# 8/.Net Core 3?
...Maybe >.>
(And since moved on to VS2022/C# 10/.Net 6)
