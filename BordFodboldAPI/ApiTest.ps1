# Set base URL
$baseUrl = "http://localhost:5008"

# Create a player
$playerBody = @{
    Name = "John Doe"
    Initials = "JD"
}
$player = Invoke-RestMethod -Uri "$baseUrl/CreatePlayer" -Method POST -ContentType "application/json" -Body ($playerBody | ConvertTo-Json)
Write-Host "Created Player:" ($player | ConvertTo-Json)

# Edit the player
$editBody = @{
    Name = "Jane Doe"
    Initials = "JD"
}
$editedPlayer = Invoke-RestMethod -Uri "$baseUrl/EditPlayer/$($player.id)" -Method POST -ContentType "application/json" -Body ($editBody | ConvertTo-Json)
Write-Host "Edited Player:" ($editedPlayer | ConvertTo-Json)

# Get all players
$players = Invoke-RestMethod -Uri "$baseUrl/GetPlayers" -Method GET
Write-Host "All Players:" ($players | ConvertTo-Json)

# Search for player
$searchTerm = "Jane"
$searchResults = Invoke-RestMethod -Uri "$baseUrl/SearchPlayers/$searchTerm" -Method GET
Write-Host "Search Results:" ($searchResults | ConvertTo-Json)

# Delete the player we created earlier
Invoke-RestMethod -Uri "$baseUrl/DeletePlayer/$($player.id)" -Method DELETE
Write-Host "Deleted Player with ID $($player.id)"

# Re-create the deleted player for a match test
$playerBody = @{
    Name = "John Doe"
    Initials = "JD"
}
$player = Invoke-RestMethod -Uri "$baseUrl/CreatePlayer" -Method POST -ContentType "application/json" -Body ($playerBody | ConvertTo-Json)
Write-Host "Created Player:" ($player | ConvertTo-Json)

# Create 3 more players for a match
$playerBody2 = @{ Name = "Alice"; Initials = "A"}
$player2 = Invoke-RestMethod -Uri "$baseUrl/CreatePlayer" -Method POST -ContentType "application/json" -Body ($playerBody2 | ConvertTo-Json)
$playerBody3 = @{ Name = "Bob"; Initials = "B"}
$player3 = Invoke-RestMethod -Uri "$baseUrl/CreatePlayer" -Method POST -ContentType "application/json" -Body ($playerBody3 | ConvertTo-Json)
$playerBody4 = @{ Name = "Charlie"; Initials = "C"}
$player4 = Invoke-RestMethod -Uri "$baseUrl/CreatePlayer" -Method POST -ContentType "application/json" -Body ($playerBody4 | ConvertTo-Json)

# Create a match
$matchBody = @{
    Team1PlayerIds = @($player2.id, $player3.id)
    Team2PlayerIds = @($player4.id, $player.id)
}
$match = Invoke-RestMethod -Uri "$baseUrl/CreateMatch" -Method POST -ContentType "application/json" -Body ($matchBody | ConvertTo-Json)
Write-Host "Created Match:" ($match | ConvertTo-Json)

# Add a result to the match we just Created
$resultBody = @{
}
$matchid = $match.ID
$result = Invoke-RestMethod -Uri "$baseUrl/AddMatchResult/$matchid/1" -Method POST -ContentType "application/json" -Body ($resultBody | ConvertTo-Json)
Write-Host "Added Match Result:" ($result | ConvertTo-Json)