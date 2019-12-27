#UnityCommon
A basis to kickstart projects with a bunch of common functionality.
This is the stuff I often use in my projects, but I figure it may be useful for others too!

##### Get the things: 

`git clone https://github.com/rossborchers/UnityCommon.git Common`    
Meta files should be removed by running Clean.bat

## Contents: 

#### Animation
  - Mechanim humanoid foot IK (2D and 3D).
  
#### Camera
  - 2D follow script with direction anticipation.

#### Physics
  - 2D and 3D velocity limiters.

#### Containers 
- Pairs and triples. 
- Quad-tree based on [Danny Goodayle's implementation](http://www.justapixel.co.uk/generic-quadtrees-for-unity/). 

#### Extensions 
- Vector swizzling by [Tyler Glaiel](http://tylerglaiel.com/). 

#### Input 
- Action and callback based input system intended to allow for arbitrary runtime mappings between inputs and actions. *Note: InputManager must have first priority in execution order.*
- **[WIP]**Runtime action to input icon retrieval for contextual prompts. Default icons by [Nicolae Berbece](http://xelubest.com/). 
- **[WIP]**Default UI for modifying bindings. 

#### MessageHandler 
- Message handler for sending messages with optional generic data to subscribed callbacks. 

