from Client import SimClient
from time import sleep

class Dolunay():
    
    SUCCESS = 0
    ERROR_OUT_OF_LOOP = 1

    inp_state : dict = {
        'inputs' : [0, 0, 500, 0],
        'set_arm' : 0,
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
        'is_armed' : 0,
    }

    # Bu özellik simulasyona eklenmediği için
    # şimdilik aracın her zaman bu mod
    # durumunda olduğunu kabul edelim
    current_mode : str = 'ACRO'

    def __init__(self):
        self.Distance = DistanceSensor(self)
        
        self.connection = SimClient()
        self.sim_data = self.connection.recv()

    def hareket_et(self, x, y, z, r, t = 1, i = 1) -> int:
        """
        x- ileri ve geri hareket,[-1000,1000] araligi(0 degeri hareket vermez)
        y- saga ve sola hareket,[-1000,1000] araligi(0 degeri hareket vermez)
        z- yukari ve asagi hareket,Uyarı! [0,1000] araligi(500 degeri hareket vermez)
        r- kendi etrafinda saga ve sola hareket,[-1000,1000] araligi(0 degeri hareket vermez)
        t- komutun kac defa gonderilecegi
        i- her komut arası beklenecek olan sure
        """
        self.inp_state['inputs'] = [x, y, z, r]

        self.connection.SendData(self.inp_state)
        self.sim_data.update(self.connection.recv())
        return self.SUCCESS

    def set_arm(self, arm : bool = True, max_try : int = 7) -> int:
        self.inp_state['set_arm'] = 1 if arm else 0
    
        for _ in range(max_try):
            self.connection.SendData(self.inp_state)
            self.sim_data.update(self.connection.recv())

            if int(self.sim_data['is_armed']) == arm:
                self.inp_state.pop('set_arm')
                print(f"-> {'ARMED' if arm else 'DISARMED'}")
                return self.SUCCESS
        return self.ERROR_OUT_OF_LOOP

    def get_mod(self) -> dict:
        data = {
            "mode": self.current_mode,
            "arm": 'ARM' if self.sim_data['is_armed'] else 'DISARM'
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
        data = float(self.master.sim_data['right_distance'])
        return data, 0.9

    def getLeftDistance(self):
        data = float(self.master.sim_data['left_distance'])
        return data, 0.9

    def getDistance(self):
        return float(self.master.sim_data['left_distance']), float(self.master.sim_data['right_distance'])

    def getDiffDis(self):
        """
        (-) değer sol sensor daha uzak
        (+) değer sağ sensor daha uzak
        """
        diff = float(self.master.sim_data['right_distance']) - float(self.master.sim_data['left_distance'])
        diff = round(diff, 5)
        return diff