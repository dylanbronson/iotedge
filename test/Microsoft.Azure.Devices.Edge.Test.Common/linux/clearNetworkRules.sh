#!/bin/bash

# This script:
# - Clears rules set on azure-iot-edge network by Linux's Traffic Controller

prefix="br-"
networkInterface=$(sudo docker inspect azure-iot-edge | grep Id | cut -c16-27)
sudo tc qdisc delete dev $prefix$networkInterface root