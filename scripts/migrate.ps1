cd ..
#dotnet ef database drop
mysql -uroot -proot -e 'drop database if exists ondrej;create database ondrej;'
if (Test-Path -Path Migrations) {
	Remove-Item -Path Migrations -Recurse -Force
} 
dotnet ef migrations add InitialCreate
dotnet ef database update
