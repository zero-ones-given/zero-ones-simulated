![Screenshot](https://repository-images.githubusercontent.com/268081145/c5953400-e350-11ea-8386-185060a80f35)

# Zero Ones Simulated
A simple simulator for a robot arena

## Quickstart
- Build and run the Unity project
- Once the simulator is running, you can get the overhead video feed (in [MJPEG format](https://en.wikipedia.org/wiki/Motion_JPEG)) from: [http://localhost:8080](http://localhost:8080)
- You can control one of the robots with your keyboard (this is enabled in the default configuration for testing purposes)
- You can control another robot by sending comma or semicolon delimited string of motor values as UDP packets to localhost port 3002

To try controlling the robot via UDP you can for example use the following command
```
echo -n '255;-255' | nc -u 127.0.0.1 3002
```

## Configuration
Once you've built the project, you can use the configuration.json file to change the configuration without the need to rebuild the project.

| Option           | Type    |
| ---------------- | ------- |
| quality          | integer |
| timeScale        | float   |
| streamFPS        | integer |
| streamResolution | integer |

## Resetting the simulation
You can reset the simulation by sending the command `reset` via UDP to the `controlPort` defined in the configuration (3000 by default). For example in Python you could do this:
```
import socket
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock.sendto(bytes("reset", "utf-8"), ("127.0.0.1", 3000))
```