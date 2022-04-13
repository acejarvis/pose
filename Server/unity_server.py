# save this as app.py
from datetime import datetime
from flask import Flask, request, send_file
import json
import subprocess
import iot

app = Flask(__name__)
ty = iot.Tuya()

previous = datetime.now()

class Position:
    def __init__(self, x, y, z):
        self.x = x
        self.y = y
        self.z = z

class Object:
    def __init__(self, name: str, position: Position):
        self.name = name
        self.position = position
@app.route("/")
def hello():
    return "Hello, World!"

@app.route('/room', methods=['GET'])
def download():
    # Appending app path to upload folder path within app root folder
    # Returning file from appended path
    return send_file("../file.json", as_attachment=True)

@app.route("/control", methods=["GET"])
def deviceControl():
    # 1s grace period
    global previous
    current = datetime.now()
    if (current - previous).seconds > 10:
        previous = current
        
        # trigger IoT command
        args = request.args
        print(args['device'])
        status = ty.getStatus(ty.devices[args['device']])
        print(status)
        if status:
            ty.turn_off(ty.switch[args['device']])
        else:
            ty.turn_on(ty.switch[args['device']])
        return "OK"

@app.route("/status", methods=["GET"])
def deviceStatus():
    args = request.args
    print(args['device'])
    return ty.getStatus(ty.switch[args['device']]).__str__()

@app.route("/server", methods=["POST"])
def server():
    data = request.get_json()
    objectList = []

    # parse objects from app
    for dataObject in data:
        if dataObject == "wall":
            counter = 0
            wallPoints = data[dataObject].split(")(")
            for wallPoint in wallPoints:
                wallPoint = wallPoint.replace("(", "")
                wallPoint = wallPoint.replace(")", "")
                wallPoint = wallPoint.split(",")
                wallPoint = [float(x) for x in wallPoint]
                outputObject = Object("wall" + str(counter), Position(wallPoint[0], wallPoint[1], -wallPoint[2]))
                objectList.append(json.dumps(outputObject, default=lambda o: o.__dict__))
                counter += 1
        else:
            objectString = data[dataObject].replace('(', '').replace(')','').split(',')
            outputObject = Object(dataObject, Position(float(objectString[0]), float(objectString[1]), -float(objectString[2])))
            objectList.append(json.dumps(outputObject, default=lambda o: o.__dict__))

    for object in objectList:
        print(object)
        print("\n")

    outputFile = '[' + ', '.join(objectList) + ']'

    file = open("../file.json", "w")
    file.write(outputFile)
    print("data received, starting server...")
    status = subprocess.Popen("../RaycastDetection.exe")
    print(status)
    return "data received, starting server", 200



if __name__ == "__main__":
    app.run(host='0.0.0.0', port=3000)