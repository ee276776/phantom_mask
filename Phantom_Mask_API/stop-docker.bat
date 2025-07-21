@echo off
echo 🛑 Stopping Phantom Mask API...
echo.

docker-compose down -v

echo.
echo ✅ All services stopped and volumes removed.
echo.
pause
