# Overview

For the develeopment of gopher, it would be advantageous if we were able to simulate the robot in the physics engine gazebo.

This is a package containing a running simulation of the gopher chest_stand component. This code was edited based on the fusion2urdf fusion360 plugin with some minor edits to work for **Noetic**.

## Updates

- **(02/14/2023)** Allowed users to spawn the stand_chest with or without the base
- **(02/13/2023)** Connected the stand to the mobile base platform - fetch freight research platform

## Dependent ROS Packages
- fetch_gazebo
- fetch_ros

- joint-state-controller
- effort-controllers
- position-controllers

# Installation
Install dependant packages for **controlling** the stand_chest:

```
sudo apt-get install ros-(distro)-joint-state-controller
sudo apt-get install ros-(distro)-effort-controllers
sudo apt-get install ros-(distro)-position-controllers
```
Replace (distro) with your ros version. Ex: noetic

Install fetch packages
```
git clone -b ros1 git@github.com:ZebraDevs/fetch_ros.git
git clone -b gazebo11 git@github.com:ZebraDevs/fetch_gazebo.git
```
where ros1 and gazebo11 are branches specifically for ros-noetic 

# Relevant Tools from Gazebo
Gazebo is a physics engine that is able to display some useful information. These are the main features that can be used to understand the robot. They can be found at the header under **View**

- Center of Mass
    - displays the center of mass of links
    - Note: links connected via a fixed joint are regarded as just 1 link
        - example is the stand link fixed to the base_link of the freight
- transparent
    - makes bodies clear
    - really useful to show the center of masses easier
- link frames
    - shows the transformations of each link

# Launch Files

This package contains 3 different launch files of interest:
- display.launch
- gazebo.launch
- controller.launch

We will go over their purpose right now.

## display.launch

This file loads the robot into rviz. This is used to make sure the robot was converted correctly from the plugin fusion2urdf. If there is no issues with the installation of the package, then there should be no errors.

To launch the file in your terminal run:

```
roslaunch stand_chest_description display.launch
```

Rviz should load with the stand_chest in the resting position (where the chest are at the lowest point 0.0).

## gazebo.launch

This file loads the robot into gazebo (either the whole robot or just the stand_chest assembly). As such we can see how the robot may act in the physics simulator (which slightly translates to the real world). As default, the whole robot is loaded into sim.

To launch the robot in your terminal run:

```
roslaunch stand_chest_description gazebo.launch
```

To just show the chest_stand assembly run:

```
roslaunch stand_chest_description gazebo.launch include_base:=false
```

As a side note, the first time gazebo runs, it may take 1 min to load. Subseqent launches of this file shoudl load quickly. 

The stand_chest will be spawned into roughtly the center of a blank world.

## controller.launch

This file loads both the robot_state_publisher and the joint_effort_controller for the robot. This controller is configured to work ONLY in sim. 

To launch the file in your terminal launch gazebo and then the controllers:

```
roslaunch stand_chest_description gazebo.launch
roslaunch stand_chest_description controller.launch
```

To check that the controller was launched successfully run:

```
rostopic list | grep "stand_chest"
```

The following should be seen:

```
/stand_chest/Zaxis_position_controller/command
/stand_chest/Zaxis_position_controller/pid/parameter_descriptions
/stand_chest/Zaxis_position_controller/pid/parameter_updates
/stand_chest/Zaxis_position_controller/state
/stand_chest/joint_states
```

As previously mentioned, this file runs:
- robot_state_publisher
- effort_joint_controller

The robot_state_publisher is renamed as /stand_chest/joint_states (check remap for more information). It reports the joint(s), effort(s), position(s), and velocity(s).

The effort_joint_controller handels the control of the joint, using the controller_manager ros package. We will be using this controller to move the chest up and down.

It is important to note that this joint is a position controller, where the controller will move the chest to target positions. 

## Controlling the stand_chest

To move the chest, in seperate terminals launch both the gazebo simulation and the controllers

```
roslaunch stand_chest_description gazebo.launch
roslaunch stand_chest_description controller.launch
```

Now, to send the chest to a target (desired) position, run:

```
rostopic pub /stand_chest/Zaxis_position_controller/command std_msgs/Float64 "data: 0.0" 
```

where data: 0.0 sends the robot to zero. 

**Valid inputs for this joint are between 0.0 and 0.47**.

## TODO

- Section detailing:
    - required packages for this to run
    - how to install the ros package
    - how to test and debug

## Develeoper debugging
 
We are going to be changing our code constantly. Particularly the urdfs and xacro. If there is any changes to these files, use the following line to debug:

```
check_urdf <(xacro model.urdf.xacro)
```

where model.urdf.xacro is the file we want to check. 

For this model, some parts need to be converted from a .xacro to a .urdf file. On **Noetic** run the following

```
rosrun xacro xacro model.xacro > model.urdf
```

where "model" is the name of the file.

### Issues, Bugs, Etc.

- In gazebo, loading the interia of the robot shows two boxes. When the chest is moving, the inertia of the stand slides down.

- For the initical control of the chest, the assembly moves too quicly to the location
    - possable fix is to limit the velocity the z_axis joint can move
