# Creating a new release

1) Create a new git tag and push it to Github.
Follow semantic versioning:
Major: potentially breaking changes
Minor: new functionality (backwards compatible)
Patch: Bug fixes (backwards compatible)

For example:
```
git tag v0.4.0
git push origin tag v0.4.0
```

2) Make a build for Mac, Linux and Windows.
Copy **configuration.json** and **Images** folder containing aruco markers 0-3. The folder structure should be as follows:
- Zero-Ones-Simulated-MacOS.zip
    - zero-ones-simulated.app
        - **configuration.json**
        - **Images**
        - Contents
- Zero-Ones-Simulated-Linux-x86-64.zip
    - **configuration.json**
    - **Images**
    - zero-ones-simulated.x86_64.x86_64
    - ...
- Zero-Ones-Simulated-Windows-x86-64.zip
    - **configuration.json**
    - **Images**
    - Zero Ones Simulated.exe
    - ...

3) Create a new release in Github and upload binaries as zipped archives