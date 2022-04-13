
from tuyapy import TuyaApi
import json

api = TuyaApi()

class Tuya:
    
    def __init__(self,parent=None):
        self.initUI()

    def turn_off(self,device):
        is_list = isinstance(device,list)
        if is_list == False:
            device.data['state'] = False
            return device.turn_off()
        else:
            for i in device:
                i.data['state'] = False
            return [i.turn_off() for i in device]

    def turn_on(self,device):
        is_list = isinstance(device,list)
        if is_list == False:
            device.data['state'] = True
            return device.turn_on()
        else:
            for i in device:
                i.data['state'] = True
            return [i.turn_on() for i in device]

    def change_colour(self,device):
        colors = QColorDialog.getColor()
        h,s,v,t = colors.getHsv()
        s = int((s/255 * 100))
        if s < 60:
            s = 60
        is_list = isinstance(device,list)
        if is_list == False:
            return device.set_color([h,s,100])
        else:
            return [i.set_color([h,s,100]) for i in device]

    def initUI(self):
        
        with open('config.json') as config:
            data = json.load(config)
            
        username,password,country_code,application = data['username'],data['password'],data['country_code'],data['application']
        api.init(username,password,country_code,application)
        self.device_ids = api.get_all_devices()
        self.switch = dict(sorted(dict((i.name(),i) for i in self.device_ids if i.obj_type == 'switch').items()))
        self.switch['All Switches'] = list(self.switch.values())
        self.lights = dict(sorted(dict((i.name(),i) for i in self.device_ids if i.obj_type == 'light').items()))
        self.lights['All Lights'] = list(self.lights.values())
        self.devices = {**self.switch,**self.lights}
        self.counter = 0

    def getStatus(self, device):
        if isinstance(device,list):
            return [i.data['state'] for i in device]
        else:
            return device.data['state']