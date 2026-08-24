# Unity CLI Workflow

The official standalone Unity CLI (`unity.exe`) is already configured and accessible in your environment.

## 1. Opening the Project
To launch the Unity Editor with this project, use:
```powershell
unity open .
```

## 2. Validating the Project Environment
To verify that your toolchain, editor versions, and licenses are healthy, use:
```powershell
unity doctor
```

To validate that the project compiles without actually running it, you can run a dry build:
```powershell
unity build .
```

## 3. Testing the Project
To run automated tests through the Unity CLI (without opening the Editor UI manually), use:
```powershell
unity test . --mode PlayMode
```
```powershell
unity test . --mode EditMode
```
*(Note: Running test or build commands via the CLI requires the Unity Editor to be closed, otherwise you will hit a project lock error).*
