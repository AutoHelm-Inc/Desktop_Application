# Desktop_Application

## Pulling and Cloning READ THIS
When u pull from Desktop_Application, use:
```
git pull --recurse-submodules
```

to update the submodule to the latest commit (i.e. use the latest automation framework when working on the desktop app) use:
```
git submodule update --remote --merge
```
and then add/push the updates (this will update to use the latest framework commit as submodule reference)

The automation framework repo can be seen from the desktop app

I would still recommend if u are making automation framework changes, pls work straight off the automation framework repo and not the desktop application repo

## Insalling Visual Studio
Download Community 2022 version of Visual Studio [here](https://visualstudio.microsoft.com/vs/)

- Check all the Desktop related sections for development. Prob not all needed, but used just in case
![Alt text](image.png)

- Add C# tools as part of Individual components
![Alt text](image-1.png)

- Run from the top here
![Alt text](image-2.png)
