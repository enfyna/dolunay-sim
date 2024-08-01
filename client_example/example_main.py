from client import Dolunay, DMS_Client
from cv2 import imshow, waitKey
from time import sleep

# To connect with the mission vehicle we will use the
# Dolunay class. 
vehicle = Dolunay()
# Dolunay class will give us some easy to use functions
# to get the informations about the vehicle.

# First we need to arm the vehicle.
vehicle.Pixhawk.set_arm(True)

vehicle.Pixhawk.set_mod('ALT_HOLD')

try:
    while True:
        # Camera
        if vehicle.Camera.is_front_cam_open():
            _, front_view = vehicle.Camera.get_front_cam()
            # Images can be used in cv2 functions
            imshow('front', front_view)
            waitKey(1);

        if vehicle.Camera.is_bottom_cam_open():
            _, bottom_view = vehicle.Camera.get_bottom_cam()
            imshow('bottom', bottom_view)
            waitKey(1);

        # Distance
        right, _ = vehicle.Distance.getRightDistance()
        left, _ = vehicle.Distance.getLeftDistance()
        # Or (same as above)
        left, right = vehicle.Distance.getDistance()

        right - left == vehicle.Distance.getDiffDis() # true

        # Pixhawk
        vehicle.Pixhawk.get_attitude() # returns yaw, roll, pitch values in a dictionary

        ps_dict = vehicle.Pixhawk.get_pressure() # returns depth distance in a dictionary
        depth = float(ps_dict['pressure'])

        print(vehicle.Distance.getDiffDis())
        vehicle.Pixhawk.hareket_et(250, 0, depth * 500, vehicle.Distance.getDiffDis() * 10)
        # Moves the vehicle according to your inputs. 
        # !IMPORTANT:This function needs to be called every loop iteration 
        # because it will update the vehicle data
except KeyboardInterrupt:
    'Ctrl + C To Exit the While Loop'

vehicle.Camera.release_cams()
vehicle.Pixhawk.kapat()
# Shutdown the vehicle and end the code.

# Note:
# If you want to create your own custom class to connect
# with the vehicle you can use the DMS_Client class.
custom = DMS_Client()
# recv() function will give you all the information about
# the vehicle in a dictionary.
# You can send your inputs using the SendData() function
# For example your movement inputs needs to be in this format:
# data['inputs'] = [x, y, z, r]
