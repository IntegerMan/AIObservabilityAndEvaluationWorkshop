#!/bin/bash
# Script to fix inotify limit issues on Linux
# This increases the inotify limit to prevent "configured user limit (128) on the number of inotify instances" errors

echo "Checking current inotify limits..."
echo "Current max_user_instances: $(cat /proc/sys/fs/inotify/max_user_instances)"
echo "Current max_user_watches: $(cat /proc/sys/fs/inotify/max_user_watches)"

echo ""
echo "Increasing inotify limits (requires sudo)..."
sudo sysctl -w fs.inotify.max_user_instances=512
sudo sysctl -w fs.inotify.max_user_watches=524288

echo ""
echo "New limits:"
echo "max_user_instances: $(cat /proc/sys/fs/inotify/max_user_instances)"
echo "max_user_watches: $(cat /proc/sys/fs/inotify/max_user_watches)"

echo ""
echo "To make these changes permanent, run:"
echo "  echo 'fs.inotify.max_user_instances=512' | sudo tee -a /etc/sysctl.conf"
echo "  echo 'fs.inotify.max_user_watches=524288' | sudo tee -a /etc/sysctl.conf"
echo "  sudo sysctl -p"


