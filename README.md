# Online Web Games
Online Web Games is a fullstack application in development that hosts simple web games that can be played both locally and online. The frontend is being devloped with React and Typescript, and the backend is being developed using .NET Core. The backend serves the frontend pages and also manages all clients, including websocket and gamestate data.

# Games
## Snake
Currently the only playable game. Snake can be played alone following standard Snake rules. It can also be played against someone else, which features both players playing synced, but separate games of snake. Each player plays their own game of Snake, but eating fruit causes the opponent's snake to get longer instead of your own. The goal is to survive longer than your opponent. 

The game is completely simulated on the server in order to prevent cheating and ensure valid states. The server updates both players' game steps at the same time and sends the new game state data to each client. The only information received from the client is when they change directions.
