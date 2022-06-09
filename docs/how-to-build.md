# How to build Raindrop Viewer

## Dependencies
- Unity LTS 2020.3.29f1
- Windows 10

## 1. Setup developer environment

This assumes you are using windows 10.
1. install LTS Unity and your favourite C# IDE (VS 2019 or rider are known to work).
2. open command prompt, input this:

    `git clone https://github.com/RaindropViewer/RaindropViewer`
3. Now you should have a folder called `RaindropViewer`.
4. open Unity Hub, and click the `ADD` button.
5. navigate to the folder `RaindropViewer` and click `Select Folder`.
6. Now, you can click the project and open it with Unity.
7. The project should load with no errors.

## 2. Run the Viewer in unity editor

1. In Unity, open up `BootstrapScene`
2. press the `Play button` at the top of unity editor.
3. The viewer should start and you can try out the app.

## 2b. (optional; sanity check) Try running some tests in Unity Test Runner.
Please ensure you download the unity test runner plugin first. (not documented here)
1. Open the test runner window: Window > General > Test Runner
2. Click `PlayMode` tab
3. You can run tests in the RaindropViewer>Tests.dll>OpenMetaverse folder. These are stable. It might take 5 minutes as a certain test takes strangely long.
4. Do note that network tests will fail unless you set an actual password in `secrets.cs`

## 3. Build for Android Device
In unity,
1. File > Build Setting 
2. ensure `Android` is selected on left.
3. ensure `Raindrop/Bootstrap/Bootstrap` is the first scene at the top. (this is the entry point of the app)
4. (optional) you can now select your android device from the list 
   
    `(make sure you turn on ADB on your phone first!)`
5. Click `Build and Run` or `Build`. 
6. if `Build and Run` succeeds, your viewer will start running on the phone! congrats :)