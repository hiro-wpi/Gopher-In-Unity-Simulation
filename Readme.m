#Launch realsense camera
roslaunch realsense2_camera rs_camera.launch
sudo rm /var/log/uvcdynctrl-udev.log


#Launch creation of mapping (gmapping)
cd catkin_ws
source devel/setup.bash
roslaunch gmapping slam_gmapping_pr2.launch

#Saving the map once completed
rosrun map_server map_saver -f mymap

#Waypoint navigation based on out map:
roslaunch fetch_navigation fetch_nav.launch map_file:=/home/wpihiro/hallway.yaml
rosrun rviz rviz -d /home/wpihiro/Maps/Waypoint_navigation.rviz

#Connecting the slave laptop to ROS_MASTER
On wpihiro
export ROS_MASTER_URI=http://192.168.0.104:11311/
export ROS_HOSTNAME=192.168.0.104
export ROS_IP=192.168.0.104
$source .bashrc


On slave laptop
export ROS_MASTER_URI=http://192.168.0.104:11311/
export ROS_HOSTNAME=192.168.0.100
export ROS_IP=192.168.0.100
$source .bashrc


#steering wheel setup
(Do this in the slave laptop)
cat /proc/bus/input/devices
## ensure that the steerig.launch file has the correct device (jsx)
roslaunch steering_wheel.launch 
#cd into workspace with python script:
~/catkin_ws/src/slam_gmapping/gmapping/src$ python steering_talker.py 
# IMPORTANT CHECK steering angle is set to 0 before launching the python file.
rostopic echo /G29/joy

#rosbridgesetup

Installed file_server form siemens

To run :
cd catkin_ws
source devel/setup.bash
roslaunch file_server publish_description_fetch.launch
#Client is on unity.


MOuse control unity representation.
roslaunch unity_simulation_scene unity_simulation_scene.launch

cd catkin_ws
source devel/setup.bash
cd src/Gopher-ROS-Unity
roslaunch gopher_unity_endpoint gopher_presence_server.launch

user: fetch 
pass: robotics
