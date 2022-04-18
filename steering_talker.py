#!/usr/bin/env python
import rospy
from geometry_msgs.msg import Twist
from sensor_msgs.msg import Joy

vel_msg_repeat = Twist()
old_data = []
def move(data):
    # Starts a new node
    #rospy.init_node('steering_talker', anonymous=True)
    old_data = data
    vel_msg = Twist()

    #Receiveing the user's input
    left_right = data.axes[0]
    foward = data.axes[2] + 1
    back = data.axes[3] + 1
    #Since we are moving just in x-axis
    vel_msg.linear.x = 1.5*foward/2 + back *-0.5/2
    vel_msg.linear.y = 0
    vel_msg.linear.z = 0
    vel_msg.angular.x = 0
    vel_msg.angular.y = 0
    vel_msg.angular.z = 3*left_right
    vel_msg_repeat = vel_msg
    rospy.loginfo(vel_msg.angular.z)
    velocity_publisher.publish(vel_msg)
    
          
def steering_talker():
	rospy.init_node('steering_talker', anonymous=True)
	rospy.Subscriber("/G29/joy", Joy, move)
	velocity_publisher.publish(vel_msg_repeat)	
	rospy.spin()
	
if __name__ == '__main__':
    try:
        #Testing our function
        velocity_publisher = rospy.Publisher('/cmd_vel', Twist, queue_size=10)
        steering_talker()
    except rospy.ROSInterruptException: pass
