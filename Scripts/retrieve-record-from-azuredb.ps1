
param ( [string]$Query,
	[string]$Connection = ("server=ServerName;database=DbName;trusted_connection=true;"),
	[switch]$help
)

function GetHelp() {


$HelpText = @"

DESCRIPTION:
NAME: Get-SQL.ps1
Queries an SQL Database and returns the information
Change the $Connection Variable on Row 12 in the script
to your Default Database.

PARAMETERS:
-Query          SELECT query (Required)
-Connection     Connection to Databse (Optional)
-help           Prints the HelpFile (Optional)

SYNTAX:
./Get-SQL.ps1 -Query "SELECT * FROM Products"

Gets All Information from the Products Table
from the Default Database and returns it to the Prompt

./Get-SQL.ps1 -Query "SELECT * FROM Products" -Connection "server=Server;database=Northwind;trusted_connection=true;"

Connects to the Specified Database and retrieves
all information from Products Table and returns it to the prompt

Get-Inventory.ps1 -help

Displays the help topic for the script

"@
$HelpText

}

function Get-SQL ([string]$Query,[string]$ConnString) {

	# Prepare the ConnectionString

	$ConnString = $ConnString.TrimStart('"')
	$ConnString = $ConnString.TrimEnd('"')

	# Connect to The SQL Server

	$Connection = New-Object System.Data.SQLClient.SQLConnection

	$Connection.ConnectionString = $ConnString
	$Connection.Open()

	# Execute the Wuery

	$Command = New-Object System.Data.SQLClient.SQLCommand
	$Command.Connection = $Connection
	$Command.CommandText = $Query

	# Add Retrieved Data to a HashTable Array

	$Reader = $Command.ExecuteReader()
	$Counter = $Reader.FieldCount
	while ($Reader.Read()) {
		$SQLObject = @{}
		for ($i = 0; $i -lt $Counter; $i++) {
			$SQLObject.Add(
				$Reader.GetName($i),
				$Reader.GetValue($i)
			);
		}

		# Return Information to Host

		$SQLObject
	}
	$Connection.Close()
}

if ($help) { GetHelp }

if ($Query -AND $Connection) { Get-SQL $Query $Connection }


$cstr = "Server=tcp:zah7c5qeen.database.windows.net,1433;Database=geek_weixin;User ID=supgk@outlook.com@zah7c5qeen;Password=Geek+IsCool;Trusted_Connection=False;Encrypt=True;Connection Timeout=30;"

Get-SQL -ConnString $cstr -Query "select * from ImageStorages"
