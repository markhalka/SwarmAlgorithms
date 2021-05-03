# SwarmAlgorithms

This repository contains a few swarm algorithm projects that I've been working on recently. The projects are often inspired by a group of papers, which I impliment and 
improve on in some way, or introduce in a novel situation. The primary goal of these projects is to learn more through applying my knowledge. 
<b>Note: This repository contains only the code from my Unity project! If you would like to run the code, you will need the entire project, please reach out to me
if this is the case</b>

### Localization and collective decision making
This project is focused on localizing a swarm of robots, using only intra-distance measurments. Below is a quick overview of the algorithm, for a complete breakdown feel free
to take a look at the paper.

The goal of a localization algorithm, is for each robot to know its position on a global coordinate frame. However, many localization algorithms rely on external
information such as GPS, or external markers to localize. In this project, there are two different implimentations. The first 'GlobalTrilateration' uses beacon nodes.
The second 'LocalTrilateration' is an algorithm I developed, which achieves localization using only intra-distance measurments between robots. Below I very briefly outline the algorithm. 
We begin with two assumptions: first, each robot has some sensor range, and can only precieve robots within its sensor range. Second, each robot has a unique ID.

1. Robots begin in a random arrangment, with no positions or coordinate systems.
2. Each robot has a small probability of becoming a seed robot. If it becomes a seed robot, then it, along with two of its neighbors create a new coordinate system.
The ID of this coordinate system is the ID of the first seed robot, and thus each coordinate system has a unique ID.
3. If a robot has no coordinate system, it searches for a group of three robots that share the same coordinate system, and trilaterates its position from them.
4. After a small amount of time has passed, each robot will belong to one of many different coordinate systems. From here, the group must decide on one.
5. Each robot has a probability of invading another coordinate system (now called regions), this probability is related to the robots position, and an enemy
robots position (where an enemy robot is any robot within range, in another coordinate system). Because each region is roughly circular, the larger the robots
magnitude of position, the larger its region is, and therefore the larger its probability of invading smaller regions. Once a robot invades a region, the invaded
robots change their coordinate systems to the invaders, using trilateration.
6. After a small amount of time, there will be a single region that has invaded all others, and this will be the global coordinate sytem
7. Optionally, each robot can further fine-tune their position using gradient descent and trilateration.

The algorithm has been expirmentally shown to work well for upto 800 robots, most of the time. 


### Clustering and Dynamic task-allocation
An interesting problem encountered often in nature, is deciding who has to do what. This problem is known as dynamic task allocation. In this project there are resources
scattered in the enviornment, and it is the ants task to 'process' all the material as effectivly as possible. This project is based on the CacheSort algorithm, and 
the threshold task-allocation algorithm.

First, lets begin with a few assumptions:
- each ant can only a small distance
- ants cannot communicate with each other
- ants can become, or join 'factories'. Factories are one or more ants who cannot move but process material, and produce more ants. The productivity of the factory 
(the amount of material processed per second) grows super=linearly with the amount of ants in the factory. Ants can independantly leave or join factories
at any time.

Now, lets look at the algorithm:
1. Ants begin by wandering around. They sort the material they see into groups, where a group is simply a collection of materials close together.
2. Ants keep track of the largest group they see as their 'cache point', this is where they will cluster material they collect.
3. Ants find the smallest group, pick up a material from that group, and bring it to their cache point. 
4. Ants have a probabiltiy, related to the amount of unprocessed material they see, to become a factory (or join a factory if there is already one present)
5. While an ant is a factory, it can process material within a short radius. The amount of materail it can process per second is related to the number of ants 
in the factory. When a factory process materail, it removes the material from the enviornment, and produces new ants (in the simulation it costs 6 materials to produce an ant)
6. While an ant is in a factory, it has a probability related to the productivity of the factory to leave. And become an ant once again


### Collective preception 
Within most animals, and escpecially humans lies an increadibly complex and powerful arsenal used to defend ourselves against pathogens - our immune system.
The immune system is essentially a swarm of cells, which must collectivly locate, identify, decide, and neutralize threats. On top of all this
the immune system (specifically the adaptive immune system) must learn and remember how to deal with new threats. This project aims to apply 
collective preception, decision making, and learning in the context of the immune system. 

Within the enviornment, there are a number of cells, which are tasked with eliminating any threats, while ignoring non-threats.
There are a number of external entities, that differ in size, shape and type. 


