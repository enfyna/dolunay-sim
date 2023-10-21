from Client import SimClient
from time import sleep

class Dolunay():
    
    SUCCESS = 0
    ERROR_OUT_OF_LOOP = 1

    state : dict = {
        'is_armed' : 0,
        'inputs' : [0, 0, 500, 0],
    }

    sim_data : dict = {
        'cam_1':'',
        'cam_2':'',
        'right_distance': 0,
        'left_distance': 0,
        'depth': 0,
        'yaw': 0,
        'roll': 0,
        'pitch': 0,
    }

    # Bu özellik simulasyona eklenmediği için
    # şimdilik aracın her zaman bu mod
    # durumunda olduğunu kabul edelim
    current_mode : str = 'ACRO'

    def __init__(self):
        self.Distance = DistanceSensor(self)
        
        self.connection = SimClient()
        self.sim_data = self.connection.recv()
        print(self.sim_data)

    def hareket_et(self, x, y, z, r, t = 1, i = 1) -> int:
        """
        x- ileri ve geri hareket,[-1000,1000] araligi(0 degeri hareket vermez)
        y- saga ve sola hareket,[-1000,1000] araligi(0 degeri hareket vermez)
        z- yukari ve asagi hareket,Uyarı! [0,1000] araligi(500 degeri hareket vermez)
        r- kendi etrafinda saga ve sola hareket,[-1000,1000] araligi(0 degeri hareket vermez)
        t- komutun kac defa gonderilecegi
        i- her komut arası beklenecek olan sure
        """
        self.state['inputs'] = [x, y, z, r]

        self.connection.SendData(self.state)
        self.sim_data.update(self.connection.recv())
        return self.SUCCESS

    def set_arm(self, arm : bool = True, max_try : int = 7) -> int:
        if arm == self.state['is_armed']:
            return self.SUCCESS
        print(f"-> {'ARMED' if arm else 'DISARMED'}")
        self.state['is_armed'] = 1 if arm else 0
        return self.SUCCESS

    def get_mod(self) -> dict:
        data = {
            "mode": self.current_mode,
            "arm": 'ARM' if self.state['is_armed'] else 'DISARM'
        }
        return data

    def getData(self) -> dict:
        lines = {}
        lines.update(self.get_attitude())
        lines.update(self.get_pressure())
        lines.update(self.get_motors())
        lines.update(self.get_mod())
        return lines

    def get_attitude(self) -> dict:
        data = {
            "yaw": self.sim_data['yaw'],
            "roll": self.sim_data['roll'],
            "pitch": self.sim_data['pitch']
        }
        return data

    def get_pressure(self) -> dict:
        data = {
            'pressure': self.sim_data['depth']
        }
        return data

    def get_motors(self) -> dict:
        """
        !!! Simulasyonda bu özellik olmadığı 
        için şimdilik 1500 değeri gönderiyor
        """
        data = {
            "servo1":1500 ,"servo2":1500 ,
            "servo3":1500 ,"servo4":1500 ,
            "servo5":1500 ,"servo6":1500 ,
            "servo7":1500 ,"servo8":1500 
        }
        return data

    def kapat(self) -> None:
        """
        Simulasyon ile olan bağlantıyı kapatır.
        """
        self.set_arm(False)

        self.connection.close()

    # Bu özellikler simulasyona eklenmediği için şimdilik birşey yapmıyor
    def set_mod(self, mode : str = 'ALT_HOLD', max_try : int = 7) -> int:
        ...

class DistanceSensor():
    def __init__(self, master):
        self.master = master

    def getRightDistance(self):
        data = self.master.sim_data['right_distance']
        return data, 0.9

    def getLeftDistance(self):
        data = self.master.sim_data['left_distance']
        return data, 0.9

    def getDistance(self):
        return self.master.sim_data['left_distance'], self.master.sim_data['right_distance']

    def getDiffDis(self):
        """
        (-) değer sol sensor daha uzak
        (+) değer sağ sensor daha uzak
        """
        diff = self.master.sim_data['right_distance'] - self.master.sim_data['left_distance']
        return diff