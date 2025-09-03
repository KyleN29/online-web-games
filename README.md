# Online Web Games
Online Web Games is a full-stack application in development that hosts simple web games that can be played both locally and online. The frontend is being developed with React and Typescript, and the backend is being developed using .NET Core. The backend serves the frontend pages and also manages all clients, including websocket and gamestate data.

# Games
## Snake
<img src="https://github.com/user-attachments/assets/caab5590-18b7-4d85-b140-64ceb83a6826" alt="snake_gif" width="300" height="300">


Snake is currently the only playable game. Snake can be played alone following standard Snake rules, or it can be played against someone else. When playing against another player, both players play separate but synced games of Snake. Each player plays their own game of Snake, but eating fruit causes the opponent's snake to get longer instead of your own. The goal is to survive longer than your opponent. 

The game is completely simulated on the server to prevent cheating and ensure valid states. The server updates both players' game steps simultaneously and sends the new game state data to each client. The only information received from the client is when they change directions.



