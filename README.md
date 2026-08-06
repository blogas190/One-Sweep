One Sweep is a 2.5D puzzle platformer

<img width="904" height="491" alt="image" src="https://github.com/user-attachments/assets/97c2e4b0-193e-42f9-b6a4-bd4720ac55d7" />

In this game you take the role of a janitor which uses a hoverboard to clean a massive company's building. Grind on rails, jump and clean all the dirt in one sweep but be careful,
one mistake can be harsh.

It is fully built in Unity. The assets were either acquired online or made by the developers or our animation team of 3 people

<img width="838" height="525" alt="image" src="https://github.com/user-attachments/assets/cb5e5479-bb4c-4973-b1ca-8337dd22ca0a" />

What mechanics were implemented in the game:
1. Movement - the player choses a direction to move in, they cannot stop or turnaround without interacting with other objects
2. Jumping
3. Speed ups - the player can choose to move faster or slower. They gradually speed up to the target speed we set and then they can choose freely to slow down or speed up.
Later they still go back to the target speed
4. Cleaning - the main mechanic of the game. When the player goes over or next to dirt they start cleaning it gradually. We calculate how much of the dirt is cleaned and then
remove it completely if the threshold was reached. All of the dirt on levels is also calculated to check what percentage of it was cleaned. The level can only be passed if 80%
of the total dirt is removed
5. Interactables - there are many other interctables the player can play around to either turn around, boost of of and so on. Rails can be grinded on to gain speed and reach new
areas of the level

<img width="958" height="472" alt="image" src="https://github.com/user-attachments/assets/a8612bfa-ca99-403d-84e2-be3c5ece73c1" />
