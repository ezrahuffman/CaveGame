# Cave Game
This is a top-down 2D exploration game. The majority of the work I have done for this game is focused on the map generation
and the systems (inventory, equipment, lighting, combat, etc...). I chose this as an artifact for my Capstone project because
 it shows the use of data structures and algorithms. To generate the caves for the map I first create a graph of nodes 
 that are all wall nodes. All the nodes are given a randomized weight so I can randomize the paths using DFS from the center 
 node of the graph. The nodes are also "collapsed" after they are visited so that the path can cross itself in more interesting ways. 
 After the path is constructed I use marching squares to render the path. This is used to create meshes (for each chunk of the map) and to
  smooth the path along with the use of subdivisions. You can find more about the marching squares algorithm [here](https://en.wikipedia.org/wiki/Marching_squares#:~:text=In%20computer%20graphics%2C%20marching%20squares,single%20data%20level%2C%20or%20isovalue).\
  \
While the cave generation tool was built when I started the capstone, I added subdivision and chunking to make the path smoother and allow for faster load times. Having one large mesh for the map meant that loading the scene took minutes to load sometimes. By chunking the mesh into blocks, as I had already done for the tiled parts of the map, I was able to cut the load times to seconds. I believe that these improvements fully meet the data structures and algorithm requirements. I also believe that while the game is still under development it shows an ability to design and develop software. 
