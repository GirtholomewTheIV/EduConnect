@echo off
if exist educonnect.db del educonnect.db
if exist educonnect.db-shm del educonnect.db-shm
if exist educonnect.db-wal del educonnect.db-wal
echo EduConnect database removed.
echo Start the application again to create and seed a new database.
pause
