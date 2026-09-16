@echo off

git init
git add .
git commit -m "Initial commit"

git branch -M main
git remote add origin https://github.com/Vonny1412/Valheim.SwapWeapons.git
git push -u origin main

echo.
echo Repository initialized and pushed to remote.
pause