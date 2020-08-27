This is a Boids simulation, the main idea is that each boid is an entity which can sense other boids in its vicinity.
The boid tries to align itself with the trajectories of the other boids it can see. The boid also moves to avoid other boids which it gets
too close too. Finally the boid attempts to get as close to the center of the positions of all the other nearby boids.
These simple behaviors create a complex flocking pattern reminiscient of bird flocks and murmurations. The behavior code was written entirely by me after
reading Craig Reynold's paper, and watching Sebastian Lague's Boids coding adventure. I also implemented an Octree to allow the boids to more efficiently 
determine which other boids are nearby. 