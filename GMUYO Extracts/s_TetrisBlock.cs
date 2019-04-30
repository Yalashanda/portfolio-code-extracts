using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class s_TetrisBlock : MonoBehaviour
{
    public enum BlockPurpose {
        NORMAL,//default purpose, behaves as normal
        VIRTUAL,//a Tetris block with this state is spawned for virtual testing purposes.  They do not render but can be used to do tests.
        DISPLAYNEXT, //blocks with this purpose are spawned in the next block area, they render but do nothing else.
        ULTIMATEEFFECT //blocks with the purpose are not controled but spawn from an ultimate
    }
    public int rotatePos = 0;
    public BlockPurpose purpose = BlockPurpose.NORMAL;
    public GameObject Rotater;
    s_Player owningPlayer = null;
    s_Block leftMostBlock;
    s_Block rightMostBlock;
    public s_BlockSpawner.TetrisTileType myShape;
    public s_Block[] ColorOneBlocks; //set of s_Blocks to be one color
    public s_Block[] ColorTwoBlocks;// set of s_Blocks to be a second color
    public bool shouldMove = true;
    bool canAccelerate = true;
    Timer canAccelerateTimer = new Timer(0.001f);
    //This list of the blocks this tetris piece contains
    public List<s_Block> containinedBlocks = new List<s_Block>();
    bool cannotMoveLeftAndRight = false;
    bool blocksRemoved = false;

    //float fall = 0;
    public float fallSpeed = 0.05f;
    bool allowRotation = true;
    bool limitRotation = false;
    Timer FindPlayer = new Timer(0.5f);

    private float continuousVerticalSpeed = 0.05f; // The speed at which the blocks move when the down button is held down
    private float continuousHorizontalSpeed = 0.075f; // The speed at which the blocks move when the left or right buttons are held down
    private float buttonDownWeightMax = 0.2f; // How long to weight before the block recognizes that a button is being held down

    private float verticalTimer = 0;
    private float horizontalTimer = 0;
    private float buttonDownWeightTimer = 0;

    private bool movedImmediateHorizontal = false;
    private bool movedImmediateVerical = false;

    bool didStart = false;
    float scaleFactor = 1;
    Timer shouldMoveTimer = new Timer(0.09f);
    List<float> containedBlocksZCoordinates = new List<float>();
    float rotatorZ = -25;
    float tetrisBlockZ = -25;
    private void Start()
    {

    
        shouldMoveTimer.SetTimerShouldCountDown(false);
        canAccelerateTimer.SetTimerShouldCountDown(false);
        GetComponent<SpriteRenderer>().enabled = false;
        onStart();
    }


    void onStart()
    {

        if (!didStart)
        {
            if (owningPlayer == null)
            {
                FindPlayer.SetTimerShouldCountDown(true);
            }
            else {
                scaleFactor = owningPlayer.GetTileList()[0].transform.localScale.x;
            }

            didStart = true;
            foreach (s_Block b in GetComponentsInChildren<s_Block>())
            {

                containinedBlocks.Add(b);
            }


            Rotater.transform.localPosition *= scaleFactor;
            //Rotater.transform.localPosition *= owningPlayer.MyTileSpawner.GetTileCOlliderSize();
            //sets the tetris parent of each s_block the Tetris block contains to this
            for (int i = 0; i < containinedBlocks.Count; i++)
            {

                containinedBlocks[i].SetTetrisParent(this);
                containinedBlocks[i].OnSpawn(owningPlayer);
                containinedBlocks[i].transform.localScale *= scaleFactor;
                containinedBlocks[i].transform.localPosition *= (scaleFactor);

            }


            if (owningPlayer != null && purpose != BlockPurpose.DISPLAYNEXT)
            {
                //align tetris block to grid
                transform.position = new Vector3(owningPlayer.GetClosestTile(transform.position).transform.position.x, transform.position.y, transform.position.z);
            }
            rotatorZ = Rotater.transform.position.z;
            tetrisBlockZ = transform.position.z;

            for (int i = 0; i < containinedBlocks.Count; i++)
            {
                containedBlocksZCoordinates.Add(containinedBlocks[i].transform.position.z);
            }

        }

    }
    public static void GetBlockColors(out s_Tile.Contents color1, out s_Tile.Contents color2) {

        //Set the blocks colors
        int startPoint = 2;
        int colorsToChooseFrom = Enum.GetValues(typeof(s_Tile.Contents)).Length;// gets the range of colors that can be chosen from
        int colorOne = UnityEngine.Random.Range(startPoint, colorsToChooseFrom);//picks a color excluding 0 which is none
        int colorTwo = UnityEngine.Random.Range(startPoint, colorsToChooseFrom);//picks a color excluding 0 which is none
        while (colorTwo == colorOne)//until the second color is not the same as the first color keep picking a different color for the second one
        {
            colorTwo = UnityEngine.Random.Range(startPoint, colorsToChooseFrom);
        }
        color1 = (s_Tile.Contents)colorOne;
        color2 = (s_Tile.Contents)colorTwo;
        //Debug.Log("Setting colors to " + color1.ToString() + " and " + color2.ToString());
    }

    void setBlockColors(s_Tile.Contents color1 = s_Tile.Contents.NONE, s_Tile.Contents color2 = s_Tile.Contents.NONE)
    {
        //Set the blocks colors
        int colorOne;
        int colorTwo;
        if (color1 == s_Tile.Contents.NONE && color2 == s_Tile.Contents.NONE)//if the default values are unchanged
        {
            Debug.Log("For object " + gameObject.name + "entering default values unchanged");
            int startPoint = 2;
            int colorsToChooseFrom = Enum.GetValues(typeof(s_Tile.Contents)).Length;// gets the range of colors that can be chosen from
            colorOne = UnityEngine.Random.Range(startPoint, colorsToChooseFrom);//picks a color excluding 0 which is none
            colorTwo = UnityEngine.Random.Range(startPoint, colorsToChooseFrom);//picks a color excluding 0 which is none
            while (colorTwo == colorOne)//until the second color is not the same as the first color keep picking a different color for the second one
            {
                colorTwo = UnityEngine.Random.Range(startPoint, colorsToChooseFrom);
            }
            for (int i = 0; i < ColorOneBlocks.Length; i++)
            {
                ColorOneBlocks[i].SetBlockType((s_Tile.Contents)colorOne);
            }
            for (int i = 0; i < ColorTwoBlocks.Length; i++)
            {
                ColorTwoBlocks[i].SetBlockType((s_Tile.Contents)colorTwo);
            }
        }
        else//if spesific values have been passed in
        {
            for (int i = 0; i < ColorOneBlocks.Length; i++)
            {
                ColorOneBlocks[i].SetBlockType(color1);
            }
            for (int i = 0; i < ColorTwoBlocks.Length; i++)
            {
                ColorTwoBlocks[i].SetBlockType(color2);
            }
        }

    }
    //when rotating the Z values sometimes are messed up as such this function will reset them to their starting values
    void resetZValues() {
        Rotater.transform.position = new Vector3(Rotater.transform.position.x, Rotater.transform.position.y, rotatorZ);
        transform.position = new Vector3(transform.position.x, transform.position.y, tetrisBlockZ);
        for (int i = 0; i < containedBlocksZCoordinates.Count; i++)
        {
            containinedBlocks[i].transform.position = new Vector3(containinedBlocks[i].transform.position.x, containinedBlocks[i].transform.position.y, containedBlocksZCoordinates[i]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (s_GameManager.Singleton.GetPauseState())
        {
            return;
        }
        if (s_GameManager.Singleton.Cheats)
        {
            if (Input.GetKeyUp(KeyCode.Space))
            {
                shouldMove = true;
            }
        }
        blockMovement();
        //UpdateIndividualScore();
        destroyBlock();
        if (shouldMoveTimer.CountDownAutoCheckBool())
        {
            shouldMove = true;
        }
        if (canAccelerateTimer.CountDownAutoCheckBool())
        {
            canAccelerate = true;
        }

        if (FindPlayer.CountDownAutoCheckBool())
        {
            owningPlayer = FindObjectOfType<s_Player>();
            //align tetris block to grid
            transform.position = new Vector3(owningPlayer.GetClosestTile(transform.position).transform.position.x, transform.position.y, transform.position.z);
            scaleFactor = owningPlayer.GetScaleFactor();
        }


    }

    public void ForceDestroy() {

        Destroy(gameObject);
        owningPlayer.SetTetrisBlock(null);
    }
    void destroyBlock() {
        if (containinedBlocks.Count < 1)
        {
            if (owningPlayer != null)
            {
                Destroy(gameObject);
                owningPlayer.SetTetrisBlock(null);
            }
        }

    }

    void blockMovement() {
        if (purpose == BlockPurpose.DISPLAYNEXT)
        {
            return;
        }
        if (owningPlayer != null)
        {
            if (owningPlayer.GetCurrentState() == s_Player.PlayerStates.MOVING || purpose == BlockPurpose.ULTIMATEEFFECT)
            {

                downwardMovement();
                if (purpose != BlockPurpose.ULTIMATEEFFECT && owningPlayer.Player != s_Player.PlayerNumber.AI)
                {
                    if (owningPlayer.Player == s_Player.PlayerNumber.ONE)
                    {

                        checkUserInputP1();
                    }
                    else
                    {
                        checkUserInputP2();
                    }
                }
                
            }

        }
    }
    void resetMovementHoldDownTimes() {
        movedImmediateHorizontal = false;
        movedImmediateVerical = false;

        // We want to make sure that the timers are reset upon letting go of the key so there's never a situation where the blocks won't move
        horizontalTimer = 0;
        verticalTimer = 0;
        buttonDownWeightTimer = 0;
        if (!blocksRemoved)
        {
            cannotMoveLeftAndRight = false;
        }
    }

    void checkUserInputP1()
    {
        if (purpose == BlockPurpose.DISPLAYNEXT)
        {
            return;
        }
        if (Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.S))
        {
            resetMovementHoldDownTimes();
        }

        if (Input.GetKey(KeyCode.S))
        {
            
                downArrow();
            
        }

        if (Input.GetKey(KeyCode.D))
        {
            rightArrow();
        }

        if (Input.GetKey(KeyCode.A))
        {

            leftArrow();
        }


        if (Input.GetKeyDown(KeyCode.W))
        {   //The up arrow rotates the block
            upArrow();
        }

    }
    void checkUserInputP2()
    {
        if (purpose == BlockPurpose.DISPLAYNEXT)
        {
            return;
        }
        if (Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.DownArrow))
        {
            resetMovementHoldDownTimes();
            /*movedImmediateHorizontal = false;
            movedImmediateVerical = false;

            // We want to make sure that the timers are reset upon letting go of the key so there's never a situation where the blocks won't move
            horizontalTimer = 0;
            verticalTimer = 0;
            buttonDownWeightTimer = 0;*/
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
                downArrow();

        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            rightArrow();
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {

            leftArrow();
        }


        if (Input.GetKeyDown(KeyCode.UpArrow))
        {   //The up arrow rotates the block

            upArrow();
        }

    }

    void upArrow() {
        if (purpose == BlockPurpose.DISPLAYNEXT || cannotMoveLeftAndRight)
        {
            return;
        }
        if (checkRotateIsValid())
        {
            setLeftAndRightMostBlocks();
            s_Block leftMostOrig = leftMostBlock;
            s_Block rightMostOrig = rightMostBlock;
            Vector3 origLeftPos = leftMostBlock.transform.position;
            Vector3 origRightPos = rightMostBlock.transform.position;
            rotate();
            setLeftAndRightMostBlocks();
            Rotater.transform.position += origLeftPos - leftMostBlock.transform.position;
            setInternalRotations();
            resetZValues();
        }
    }
    void leftArrow()
    {
        if (purpose == BlockPurpose.DISPLAYNEXT || cannotMoveLeftAndRight)
        {
            return;
        }

        if (movedImmediateHorizontal)
        {
            if (buttonDownWeightTimer < buttonDownWeightMax)
            {
                buttonDownWeightTimer += Time.deltaTime;
                return;
            }

            if (horizontalTimer < continuousHorizontalSpeed)
            {
                horizontalTimer += Time.deltaTime;
                return; // If the horizontal timer is less than the continuous horizontal speed, it won't run the rest of the code below
            }
        }

        if (!movedImmediateHorizontal)
        {
            movedImmediateHorizontal = true;
        }

        horizontalTimer = 0;
        if (checkIsValidPositionToLeft())
        {
            transform.position += new Vector3(-owningPlayer.GetTileXOffset() * owningPlayer.GetAspectRatioScaleFactor(), 0, 0);
           
        }
        resetZValues();

    }
    void rightArrow() {
        if (purpose == BlockPurpose.DISPLAYNEXT || cannotMoveLeftAndRight)
        {
            return;
        }
        float zcoor = transform.position.z;
        if (movedImmediateHorizontal)
        {
            if (buttonDownWeightTimer < buttonDownWeightMax)
            {
                buttonDownWeightTimer += Time.deltaTime;
                return;
            }

            if (horizontalTimer < continuousHorizontalSpeed)
            {
                horizontalTimer += Time.deltaTime;
                return; // If the horizontal timer is less than the continuous horizontal speed, it won't run the rest of the code below
            }
        }

        if (!movedImmediateHorizontal)
        {
            movedImmediateHorizontal = true;
        }

        horizontalTimer = 0;
        if (checkIsValidPositionToRight())
        {
            transform.position += new Vector3(owningPlayer.GetTileXOffset() * owningPlayer.GetAspectRatioScaleFactor(), 0, 0);
         
        }
        resetZValues();
    }

    void downwardMovement() {
        if (purpose == BlockPurpose.DISPLAYNEXT)
        {
            return;
        }
        if (owningPlayer != null && shouldMove)
        {
            transform.position += (new Vector3(0, -0.5f, 0) * Time.deltaTime * owningPlayer.GetFallSpeed() * scaleFactor);
        }
    }

    void downArrow() {
        if (purpose == BlockPurpose.DISPLAYNEXT || !canAccelerate || !checkAccelerateIsVaid())
        {
            return;
        }
        cannotMoveLeftAndRight = true;
        if (movedImmediateVerical)
        {
            if (buttonDownWeightTimer < buttonDownWeightMax)
            {
                buttonDownWeightTimer += Time.deltaTime;
                return;
            }

            if (verticalTimer < continuousVerticalSpeed)
            {
                verticalTimer += Time.deltaTime;
                return; // If the vertical timer is less than the continuous vertical speed, it won't run the rest of the code below
            }
        }

        if (!movedImmediateVerical)
        {
            movedImmediateVerical = true;
        }

        verticalTimer = 0;
        transform.position += new Vector3(0, -owningPlayer.GetTileXOffset(), 0);


    }


    bool checkIsValidPosition()
    {
        if (purpose == BlockPurpose.DISPLAYNEXT)
        {
            return false;
        }
        s_Tile bottomLeft = owningPlayer.GetTileList()[0];
        s_Tile oneInFromTopRight = owningPlayer.GetTileList()[owningPlayer.GetTileList().Count - 2];
        if (transform.position.x > bottomLeft.transform.position.x &&
            transform.position.x < oneInFromTopRight.transform.position.x
            )
        {
            return true;
        }
        else
        {

            if (transform.position.x < bottomLeft.transform.position.x)
            {
                transform.position = new Vector2(bottomLeft.transform.position.x, transform.position.y);
            }

            if (transform.position.x > oneInFromTopRight.transform.position.x)
            {
                transform.position = new Vector2(oneInFromTopRight.transform.position.x, transform.position.y);
            }
            return false;
        }

    }
    bool checkIsValidPositionToRight()
    {
        if (purpose == BlockPurpose.DISPLAYNEXT)
        {
            return false;
        }
        s_Tile bottomLeft = owningPlayer.GetTileList()[0];
        s_Tile oneInFromTopRight = GetTetrisBlockShape() == s_BlockSpawner.TetrisTileType.COLUMN ? owningPlayer.GetTileList()[owningPlayer.GetTileList().Count - 1] : owningPlayer.GetTileList()[owningPlayer.GetTileList().Count - 2];

        if (GetTetrisBlockShape() == s_BlockSpawner.TetrisTileType.COLUMN &&
            rotatePos % 2 == 1
            )
        {
            oneInFromTopRight = owningPlayer.GetTileList()[owningPlayer.GetTileList().Count - 2];            
        }

        if (s_GameManager.GetDebug())
        {
            Debug.Log("Right most is " + (GetRightMostBlock().transform.position.x + owningPlayer.GetTileXOffset() + " compared to " + (owningPlayer.GetRightMostColumnXPos() + (Mathf.Abs(owningPlayer.GetRightMostColumnXPos()) * 0.25f)).ToString()));
        }
        if (GetRightMostBlock().transform.position.x + owningPlayer.GetTileXOffset() > owningPlayer.GetRightMostColumnXPos() + (Mathf.Abs(owningPlayer.GetRightMostColumnXPos()) * 0.25f))
        {
            return false;
        }

        if (GetRightMostBlock().transform.position.x < oneInFromTopRight.transform.position.x
            && isSpaceToRightEmpty())
        {
            return true;
        }
        else
        {

            if (transform.position.x < bottomLeft.transform.position.x)
            {
                transform.position = new Vector2(bottomLeft.transform.position.x, transform.position.y);
            }

            if (transform.position.x > oneInFromTopRight.transform.position.x)
            {
                transform.position = new Vector2(oneInFromTopRight.transform.position.x, transform.position.y);
            }
            return false;
        }

    }
    bool checkIsValidPositionToLeft()
    {
        if (purpose == BlockPurpose.DISPLAYNEXT)
        {
            return false;
        }

        s_Tile bottomLeft = owningPlayer.GetTileList()[0];
        s_Tile oneInFromTopRight = owningPlayer.GetTileList()[owningPlayer.GetTileList().Count - 2];
        if (transform.position.x > bottomLeft.transform.position.x &&
            isSpaceToLeftEmpty())
        {
            return true;
        }
        else
        {

            if (transform.position.x < bottomLeft.transform.position.x)
            {
                transform.position = new Vector2(bottomLeft.transform.position.x, transform.position.y);
            }

            if (transform.position.x > oneInFromTopRight.transform.position.x)
            {
                transform.position = new Vector2(oneInFromTopRight.transform.position.x, transform.position.y);
            }
            return false;
        }

    }

    bool checkAccelerateIsVaid() {
        if (purpose == BlockPurpose.DISPLAYNEXT)
        {
            return false;
        }
        if (transform.position.y <= owningPlayer.GetTileList()[owningPlayer.GetColumnCount() * 2].transform.position.y)
        {
            canAccelerate = false;
        }
        for (int i = 0; i < containinedBlocks.Count; i++)
        {
            if (containinedBlocks[i].enteredTile != null)
            {
                s_Tile belowTile = containinedBlocks[i].enteredTile.GetDownTile();
                if (belowTile == null ||
                    belowTile.GetContents() != s_Tile.Contents.NONE ||
                    belowTile.GetDownTile() == null ||
                    belowTile.GetDownTile().GetContents() != s_Tile.Contents.NONE)
                {
                    return false;
                }
            }

        }
        return true;
    }
    bool resetZAndReturnFalse() {
        
        resetZValues();
        return false;

    }
    bool checkRotateIsValid()
    {
        if (purpose == BlockPurpose.DISPLAYNEXT)
        {
            return false;
        }
        switch (myShape)
        {
            case s_BlockSpawner.TetrisTileType.SQUARE:
                return true;
            case s_BlockSpawner.TetrisTileType.ZIGZAG:
                return rotaCheckBasic();
            case s_BlockSpawner.TetrisTileType.COLUMN:
                return rotaCheckI();
            case s_BlockSpawner.TetrisTileType.L:
                return rotaCheckBasic();
            case s_BlockSpawner.TetrisTileType.BACKWARDSL:
                return rotaCheckBasic();
            case s_BlockSpawner.TetrisTileType.BACKWARDSZIGZAG:
                return rotaCheckBasic();
            case s_BlockSpawner.TetrisTileType.T:
                return rotaCheckT();
            default:
                return false;
        }


    }
    bool rotaCheckI() {

        switch (rotatePos)
        {
            case 0:
                return IRotationCheck();
            case 1:
                return true;
            case 2:
                return IRotationCheck();
            case 3:
                return true;
            default:
                return false;
        }

        
    }
    bool IRotationCheck() {
        if (GetLowestSBlock().GetEnteredTile() != null && GetLowestSBlock().GetEnteredTile().GetColumnIn() < owningPlayer.GetColumnCount() - 5)
        {
            if (oneOverIsSpaceToRightEmpty())
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            if (checkIsValidPositionToLeft())
            {
                if (GetLowestSBlock().GetEnteredTile() != null)
                {
                    int timesToRepeat = 3 - ((owningPlayer.GetColumnCount() - 1) - GetLowestSBlock().GetEnteredTile().GetColumnIn());
                    for (int i = 0; i < timesToRepeat; i++)
                    {
                        AIMoveLeft();
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

    }
    bool rotaCheckBasic()
    {
        switch (rotatePos)
        {
            case 0:
                return basicRotationCheck();
            case 1:
                return true;
            case 2:
                return basicRotationCheck();
            case 3:
                return true;
            default:
                return false;
        }
    }
    bool basicRotationCheck() {
        if (GetLeftMostBlock().GetEnteredTile() != null && GetLeftMostBlock().GetEnteredTile().GetColumnIn() < owningPlayer.GetColumnCount() - 4)
        {
            if (isSpaceToRightEmpty())
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        else
        {
            if (checkIsValidPositionToLeft())
            {
                if (GetLeftMostBlock().GetEnteredTile() != null && owningPlayer != null)
                {
                    int timesToRepeat = 2 - ((owningPlayer.GetColumnCount() - 1) - GetLeftMostBlock().GetEnteredTile().GetColumnIn());
                    for (int i = 0; i < timesToRepeat; i++)
                    {
                        AIMoveLeft();
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

        }
    }
    bool TRotationCheck() {
        if (GetLeftMostBlock().GetEnteredTile() != null && GetLeftMostBlock().GetEnteredTile().GetColumnIn() < owningPlayer.GetColumnCount() - 4)
        {
            if (isSpaceToRightEmpty())
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            if (checkIsValidPositionToLeft())
            {
                if (GetLeftMostBlock().GetEnteredTile() != null && owningPlayer != null)
                {
                    int timesToRepeat = 2 - ((owningPlayer.GetColumnCount() - 1) - GetLeftMostBlock().GetEnteredTile().GetColumnIn());
                    for (int i = 0; i < timesToRepeat; i++)
                    {
                        AIMoveLeft();
                    }
                    return true;
                }
                else
                {
                    return false;
                }
                  
            }
            else
            {
                return false;
            }
        }
    }
    bool rotaCheckT()
    {
        switch (rotatePos)
        {
            case 0:
                return TRotationCheck();
            case 1:
                return true;
            case 2:
                return TRotationCheck();
            case 3:
                return true;
            default:
                return false;
        }
    }

    bool checkRotateIsValidOrig() {
        if (purpose == BlockPurpose.DISPLAYNEXT)
        {
            return false;
        }
        

        List<s_Tile> tilesToCheckContentOf = new List<s_Tile>();
        for (int i = 0; i < containinedBlocks.Count; i++)
        {
            containinedBlocks[i].SetCanOccupyTile(false);
        }

        rotate();

        setLeftAndRightMostBlocks();
        for (int i = 0; i < containinedBlocks.Count; i++)
        {
            tilesToCheckContentOf.Add(owningPlayer.GetClosestTile(containinedBlocks[i].transform.position));
         
        }
        float offset = 0.44f;
        if ((myShape == s_BlockSpawner.TetrisTileType.COLUMN ? rightMostBlock.transform.position.x + offset : rightMostBlock.transform.position.x) > owningPlayer.GetRightMostColumnXPos())
        {
            rotateBack();
            return resetZAndReturnFalse();
        }

        rotateBack();
        for (int i = 0; i < containinedBlocks.Count; i++)
        {
            containinedBlocks[i].SetCanOccupyTile(true);
        }

        for (int i = 0; i < tilesToCheckContentOf.Count; i++)
        {
           /* if (tilesToCheckContentOf[i] == null ||
                tilesToCheckContentOf[i].GetContents() != s_Tile.Contents.NONE ||
                tilesToCheckContentOf[i].GetTileOccupant() != null)
            {
                return resetZAndReturnFalse();
            }*/
            for (int j = 0; j < tilesToCheckContentOf.Count; j++)
            {
                if (tilesToCheckContentOf[j] == tilesToCheckContentOf[i] && i != j)
                {
                    return resetZAndReturnFalse();
                }
            }

        }
        resetZValues();
        return true;
    }

    void decreaseRotatePos() {
        rotatePos--;
        if (rotatePos < 0)
        {
            rotatePos = 3;
        }
    }
    void increaseRotatePos()
    {
        rotatePos++;
        if (rotatePos > 3)
        {
            rotatePos = 0;
        }
    }

    void rotate() {
        if (purpose == BlockPurpose.DISPLAYNEXT)
        {
            return;
        }

        if (allowRotation)
        {
            if (limitRotation) // This is if we want certain blocks to ONLY be at 90 and -90 degrees
            {
                if (Rotater.transform.rotation.eulerAngles.z >= 90)
                {
                    Rotater.transform.Rotate(0, 0, -90);
                    decreaseRotatePos();
                }

                else
                {
                    Rotater.transform.Rotate(0, 0, 90);
                    increaseRotatePos();
                }
            }

            else
            {
                Rotater.transform.Rotate(0, 0, 90);
                increaseRotatePos();
            }

            if (!checkIsValidPosition())
            {
           
                if (limitRotation)
                {
                    if (Rotater.transform.rotation.eulerAngles.z >= 90)
                    {
                        Rotater.transform.Rotate(0, 0, -90);
                        decreaseRotatePos();
                    }
                    else
                    {
                        Rotater.transform.Rotate(0, 0, 90);
                        increaseRotatePos();
                    }
                }
                else
                {
                    Rotater.transform.Rotate(0, 0, -90);
                    decreaseRotatePos();
                }
            }


        }
  
    }
    void rotateBack()
    {
        if (purpose == BlockPurpose.DISPLAYNEXT)
        {
            return;
        }
        if (allowRotation)
        {
            if (limitRotation) // This is if we want certain blocks to ONLY be at 90 and -90 degrees
            {
                if (Rotater.transform.rotation.eulerAngles.z <= -90)
                {
                    Rotater.transform.Rotate(0, 0, 90);
                    increaseRotatePos();
                }

                else
                {
                    Rotater.transform.Rotate(0, 0, -90);
                    decreaseRotatePos();
                }
            }

            else
            {
                Rotater.transform.Rotate(0, 0, -90);
                decreaseRotatePos();
            }

            if (!checkIsValidPosition())
            {
                if (limitRotation)
                {
                    if (Rotater.transform.rotation.eulerAngles.z <= -90)
                    {
                        Rotater.transform.Rotate(0, 0, 90);
                        increaseRotatePos();
                    }
                    else
                    {
                        Rotater.transform.Rotate(0, 0, -90);
                        decreaseRotatePos();
                    }
                }
                else
                {
                    Rotater.transform.Rotate(0, 0, 90);
                    increaseRotatePos();
                }
            }
        }
 
    }
    bool isSpaceToLeftEmpty() {

        if (purpose == BlockPurpose.DISPLAYNEXT)
        {
            return false;
        }

        for (int i = 0; i < containinedBlocks.Count; i++)
        {
            if (containinedBlocks[i].GetLastTileEntered() != null) {
                if (containinedBlocks[i].GetLastTileEntered().GetLeftTile() == null ||
                    containinedBlocks[i].GetLastTileEntered().GetLeftTile().GetContents() != s_Tile.Contents.NONE)
                {
                    return false;
                }
            }

        }


        return true;
    }
    bool isSpaceToRightEmpty()
    {
        if (purpose == BlockPurpose.DISPLAYNEXT)
        {
            return false;
        }
        
        for (int i = 0; i < containinedBlocks.Count; i++)
        {
            if (containinedBlocks[i].GetLastTileEntered() != null)
            {
                if (containinedBlocks[i].GetLastTileEntered().GetRightTile() == null ||
                containinedBlocks[i].GetLastTileEntered().GetRightTile().GetContents() != s_Tile.Contents.NONE)
                {
                    return false;
                }
            }

        }
        return true;
    }
    bool oneOverIsSpaceToRightEmpty()
    {
        if (purpose == BlockPurpose.DISPLAYNEXT)
        {
            return false;
        }

        for (int i = 0; i < containinedBlocks.Count; i++)
        {
            if (containinedBlocks[i].GetLastTileEntered() != null)
            {
                if (containinedBlocks[i].GetLastTileEntered().GetRightTile() == null ||
                    containinedBlocks[i].GetLastTileEntered().GetRightTile().GetRightTile() == null ||
                    containinedBlocks[i].GetLastTileEntered().GetRightTile().GetRightTile().GetContents() != s_Tile.Contents.NONE)
                {
                    return false;
                }
            }

        }
        return true;
    }

    public void Rotate() {
        upArrow();
    }

    void setInternalRotations()
    {
        for (int i = 0; i < GetContainedBlocksArray().Count; i++)
        {
            Vector3 r = GetContainedBlocksArray()[i].transform.localEulerAngles;
            GetContainedBlocksArray()[i].transform.localEulerAngles = new Vector3();
            switch (rotatePos)
            {
                case 0:
                    r = new Vector3(r.x, r.y, 0);
                    break;
                case 1:
                    r = new Vector3(r.x, r.y, -90);
                    break;
                case 2:
                    r = new Vector3(r.x, r.y, 180);
                    break;
                case 3:
                    r = new Vector3(r.x, r.y, 90);
                    break;
                default:
                    Debug.LogError("rotatePos not within the 0-3 range!");
                    break;
            }


            GetContainedBlocksArray()[i].transform.localEulerAngles = r;
        }
    }
        

    //sets the owning player as the one that spawned it
    public void OnSpawn(s_Player owner, s_Tile.Contents c1, s_Tile.Contents c2, s_BlockSpawner.TetrisTileType typeOfBlock, BlockPurpose myPurpose) {
        purpose = myPurpose;
        owningPlayer = owner;


        onStart();
        //Sets the colors for the s_blocks that make up this s_TetrisBlock
        setBlockColors(c1, c2);
        myShape = typeOfBlock;
        if (purpose == BlockPurpose.NORMAL)
        {
            owningPlayer.SetTetrisBlock(this);
        }
        if (purpose == BlockPurpose.DISPLAYNEXT)
        {
            for (int i = 0; i < containinedBlocks.Count; i++)
            {
                containinedBlocks[i].SetStateToStopped();
            }
        }


    }

    public void RemoveBlock(s_Block toRemove) {
        if (purpose != BlockPurpose.NORMAL)
        {
            return;
        }
        int toRemoveColumn = toRemove.GetEnteredTile().GetColumnIn();
        if (containinedBlocks.Contains(toRemove))
        {
            containinedBlocks.Remove(toRemove);
        }
        if (containedBlocksZCoordinates.Contains(toRemove.transform.position.z))
        {
            containedBlocksZCoordinates.Remove(toRemove.transform.position.z);
        }
        for (int i = 0; i < containinedBlocks.Count; i++)
        {
            if (containinedBlocks[i] != null && containinedBlocks[i].GetEnteredTile() != null && containinedBlocks[i].GetEnteredTile().GetColumnIn() == toRemoveColumn)
            {
                containinedBlocks[i].BeginStopInEnteredTile();
            }
        }

        cannotMoveLeftAndRight = true;
        blocksRemoved = true;
        transform.position += new Vector3(0, 0.01f, 0);
        canAccelerate = false;
        canAccelerateTimer.SetTimerShouldCountDown(true);
    }

    public void PauseMovement() {
        shouldMove = false;
        shouldMoveTimer.SetTimerShouldCountDown(true);

    }

    public BlockPurpose GetPurpose() {
        return purpose;
    }

    void setLeftAndRightMostBlocks() {
        if (containinedBlocks.Count > 0)
        {
            leftMostBlock = containinedBlocks[0];
            rightMostBlock = containinedBlocks[0];
            for (int i = 0; i < containinedBlocks.Count; i++)
            {
                if (containinedBlocks[i] != null && leftMostBlock != null)
                {
                    if (containinedBlocks[i].transform.position.x < leftMostBlock.transform.position.x)
                    {
                        leftMostBlock = containinedBlocks[i];
                    }
                    if (containinedBlocks[i].transform.position.x > rightMostBlock.transform.position.x)
                    {
                        rightMostBlock = containinedBlocks[i];
                    }
                }
            }

        }
    }
    //returns the difference between the y positions of the highest and lowest s_blocks that make up this s_TetrisBlock
    public float GetHighestAndLowestDifferance() {

        int highestPos = 0;
        int lowestPos = 0;
        for (int i = 0; i < containinedBlocks.Count; i++)
        {
            if (containinedBlocks[i].transform.position.y > containinedBlocks[highestPos].transform.position.y)
            {
                highestPos = i;
            }

            if (containinedBlocks[i].transform.position.y < containinedBlocks[lowestPos].transform.position.y)
            {
                lowestPos = i;
            }

        }

        return containinedBlocks[highestPos].transform.position.y - containinedBlocks[lowestPos].transform.position.y;
    }
    public s_Block GetLeftMostBlockOfColor(s_Tile.Contents colorToLookAt) {
        if (containinedBlocks.Count > 0)
        {
            s_Block leftMostBlockColorBased = null;
            if (ColorTwoBlocks[0].GetBlockType() == colorToLookAt)
            {
                leftMostBlockColorBased = ColorTwoBlocks[0];
            }
            else if (ColorOneBlocks[0].GetBlockType() == colorToLookAt)
            {
                leftMostBlockColorBased = ColorOneBlocks[0];
            }
            else {
                return null;
            }

            for (int i = 0; i < containinedBlocks.Count; i++)
            {
                if (containinedBlocks[i].GetBlockType() == colorToLookAt)
                {
                    if (containinedBlocks[i].transform.position.x < leftMostBlockColorBased.transform.position.x)
                    {
                        leftMostBlockColorBased = containinedBlocks[i];
                    }
                }
            }
            return leftMostBlockColorBased;
        }

        return null;

    }
    public s_Block GetBottomMostBlockOfColor(s_Tile.Contents colorToLookAt)
    {
        if (containinedBlocks.Count > 0)
        {
            s_Block bottomMostBlockColorBased = null;
            if (ColorTwoBlocks[0].GetBlockType() == colorToLookAt)
            {
                bottomMostBlockColorBased = ColorTwoBlocks[0];
            }
            else if (ColorOneBlocks[0].GetBlockType() == colorToLookAt)
            {
                bottomMostBlockColorBased = ColorOneBlocks[0];
            }
            else
            {
                return null;
            }

            for (int i = 0; i < containinedBlocks.Count; i++)
            {
                if (containinedBlocks[i].GetBlockType() == colorToLookAt)
                {
                    if (containinedBlocks[i].transform.position.y < bottomMostBlockColorBased.transform.position.y)
                    {
                        bottomMostBlockColorBased = containinedBlocks[i];
                    }
                }
            }
            return bottomMostBlockColorBased;
        }

        return null;

    }
    public int PostRotationColumnTest(s_Block toCheck) {
        return owningPlayer.GetClosestTile(toCheck.transform.position).GetColumnIn();
    }
    public int PostRotationRowTest(s_Block toCheck)
    {
        int val = owningPlayer.GetClosestTile(toCheck.transform.position).GetRowIn();
        //Debug.Log(toCheck.name + " is in row " + val);
        return val;
    }
    public bool ColorInSameColumn(s_Tile.Contents colorToLookAt) {
        if (ColorTwoBlocks[0].GetBlockType() == colorToLookAt)
        {
            for (int i = 0; i < ColorTwoBlocks.Length; i++)
            {
                
                if (PostRotationColumnTest(ColorTwoBlocks[0]) != PostRotationColumnTest(ColorTwoBlocks[i]))
                {
                    return false;
                }
            }
            return true;
        }
        else if (ColorOneBlocks[0].GetBlockType() == colorToLookAt)
        {
            
            for (int i = 0; i < ColorOneBlocks.Length; i++)
            {
                if (PostRotationColumnTest(ColorOneBlocks[0])!= PostRotationColumnTest(ColorOneBlocks[i]))
                {
                    return false;
                }
            }
            return true;
        }
        else
        {
            Debug.Log("Neither color matched");
            return false;
        }
    }
    public bool ColorInSameRow(s_Tile.Contents colorToLookAt)
    {
        if (ColorTwoBlocks[0].GetBlockType() == colorToLookAt)
        {
            for (int i = 0; i < ColorTwoBlocks.Length; i++)
            {
                if (ColorTwoBlocks[0] != ColorTwoBlocks[i])
                {
                    if (PostRotationRowTest(ColorTwoBlocks[0]) != PostRotationRowTest(ColorTwoBlocks[i]))
                    {
                        return false;
                    }
                }
                
            }
            return true;
        }
        else if (ColorOneBlocks[0].GetBlockType() == colorToLookAt)
        {

            for (int i = 0; i < ColorOneBlocks.Length; i++)
            {
                if (ColorOneBlocks[0] != ColorOneBlocks[i])
                {
                    if (PostRotationRowTest(ColorOneBlocks[0]) != PostRotationRowTest(ColorOneBlocks[i]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        else
        {
            return false;
        }
    }
    public List<s_Block> GetContainedBlocksArray() {
        return containinedBlocks;
    }

    //gets an s_Block at a relative position in the s_tetrisBlock
    public s_Block GetLowestLeftMostSBlock() {
        List<s_Block> leftest = new List<s_Block>();
        int leftestPos = 0;
        for (int i = 0; i < containinedBlocks.Count; i++)
        {
            if (containinedBlocks[i].transform.position.x < containinedBlocks[leftestPos].transform.position.x)
            {
                if (i < containinedBlocks.Count)
                {
                    leftestPos = i;
                }
            }
        }

        if (containinedBlocks.Count > leftestPos && leftestPos > -1)
        {
            leftest.Add(containinedBlocks[leftestPos]);
        }
        else
        {
            Debug.LogError("Left most list position " + leftestPos + " is out of bounds or negative against count of " + containinedBlocks.Count);
        }
        

        foreach (s_Block b in containinedBlocks)
        {

            if (b != containinedBlocks[leftestPos] &&
                s_Calculator.AreNear(b.transform.position.x, containinedBlocks[leftestPos].transform.position.x, 0.001f)
                )
            {
                leftest.Add(b);
            }
        }
        int lowest = 0;
        for (int i = 0; i < leftest.Count; i++)
        {
            if (leftest[i].transform.position.y < containinedBlocks[lowest].transform.position.y)
            {
                lowest = i;
            }
        }

        if (lowest < leftest.Count)
        {
            return leftest[lowest];
        }
        else
        {
            Debug.Log("GetLowestRightMostSBlock could not return value at index " + lowest + "for list of length " + leftest.Count);
            return leftest.Count > 0 ? leftest[0] : GetContainedBlocksArray()[0];
        }
       
    }
    public s_Block GetLowestRightMostSBlock()
    {
        List<s_Block> rightest = new List<s_Block>();
        int rightestPos = 0;
        for (int i = 0; i < containinedBlocks.Count; i++)
        {
            if (containinedBlocks[i].transform.position.x > containinedBlocks[rightestPos].transform.position.x)
            {
                rightestPos = i;
            }
        }

        if (containinedBlocks.Count > rightestPos && rightestPos > -1)
        {
            rightest.Add(containinedBlocks[rightestPos]);
        }
        foreach (s_Block b in containinedBlocks)
        {

            if (b != containinedBlocks[rightestPos] &&
                s_Calculator.AreNear(b.transform.position.x, containinedBlocks[rightestPos].transform.position.x, 0.001f)
                )
            //owningPlayer.GetClosestTile(b.transform).GetColumnIn() == owningPlayer.GetClosestTile(containinedBlocks[rightestPos].transform.position).GetColumnIn()
            {
                rightest.Add(b);
            }
        }
        int lowest = 0;
        for (int i = 0; i < rightest.Count; i++)
        {
            if (i < rightest.Count)
            {
                if (rightest[i].transform.position.y < containinedBlocks[lowest].transform.position.y)
                {
                    lowest = i;
                }
            }
        }

        if (lowest < rightest.Count)
        {
            return rightest[lowest];
        }
        else
        {
            Debug.Log("GetLowestRightMostSBlock could not return value at index " + lowest + "for list of length " + rightest.Count);
            if (rightest.Count > 0)
            {
                return rightest[0];
            }
            else
            {
                return containinedBlocks[0];
            }
        

        }
    }
    public s_Block GetLowestSBlock() {  
            int lowestPos = 0;
            for (int i = 0; i < containinedBlocks.Count; i++)
            {
                if (containinedBlocks[i].transform.position.y < containinedBlocks[lowestPos].transform.position.y)
                {
                    lowestPos = i;
                }

            }
        if (lowestPos < containinedBlocks.Count && lowestPos > -1)
        {
            return containinedBlocks[lowestPos];
        }
        else
        {
            Debug.Log("GetLowestSBlock could not return value at index " + lowestPos + "for list of length " + containinedBlocks.Count);
            if (containinedBlocks.Count > 0)
            {
                return containinedBlocks[0];
            }
            else
            {
                return null;
            }
        }
        
        

    }
    public s_Block GetHighestSBlock()
    {
        int highestPos = 0;
        for (int i = 0; i < containinedBlocks.Count; i++)
        {
            if (containinedBlocks[i].transform.position.y > containinedBlocks[highestPos].transform.position.y)
            {
                highestPos = i;
            }

        }

        return containinedBlocks[highestPos];


    }
    public s_Block GetLeftMostBlock() {
        setLeftAndRightMostBlocks();
        return leftMostBlock;
    }
    public s_Block GetRightMostBlock()
    {
        setLeftAndRightMostBlocks();
        return rightMostBlock;
    }
    //returns the shape of the s_TetrisBlock s, j, l, t, etc..
    public s_BlockSpawner.TetrisTileType GetTetrisBlockShape() {
        return myShape;
    }

    public bool DoAllContainedBlocksHaveEnteredTile() {
        for (int i = 0; i < containinedBlocks.Count; i++)
        {
            if (containinedBlocks[i].GetLastTileEntered() == null)
            {
                return false;
            }
        }
        return true;
    }

    public void AIMoveLeft() {
        leftArrow();
        movedImmediateHorizontal = false;
        movedImmediateVerical = false;

        // We want to make sure that the timers are reset upon letting go of the "key" so there's never a situation where the blocks won't move, this key let go needs to be simulated here since the AI is not actually pressing a key
        horizontalTimer = 0;
        verticalTimer = 0;
        buttonDownWeightTimer = 0;
        for (int i = 0; i < containinedBlocks.Count; i++)
        {
            if (containinedBlocks[i].enteredTile != null)
            {
                if (containinedBlocks[i].enteredTile.GetLeftTile() != null)
                {
                    containinedBlocks[i].SetLastTileEntered(containinedBlocks[i].enteredTile.GetLeftTile());
                }
                else
                {
                    containinedBlocks[i].SetLastTileEntered(owningPlayer.GetClosestTile(containinedBlocks[i].transform.position));
                }
            }
            else
            {
                containinedBlocks[i].SetLastTileEntered(owningPlayer.GetClosestTile(containinedBlocks[i].transform.position));
                Debug.LogWarning("enteredTile check in AIMoveLeft was found to be null! Setting enteredtile to nearest tile");
            }
            
        }
    }
    public void AIMoveRight()
    {
        rightArrow();
        movedImmediateHorizontal = false;
        movedImmediateVerical = false;

        // We want to make sure that the timers are reset upon letting go of the "key" so there's never a situation where the blocks won't move, this key let go needs to be simulated here since the AI is not actually pressing a key
        horizontalTimer = 0;
        verticalTimer = 0;
        buttonDownWeightTimer = 0;
        for (int i = 0; i < containinedBlocks.Count; i++)
        {
            if (containinedBlocks[i].enteredTile != null)
            {
               if (containinedBlocks[i].enteredTile.GetRightTile() != null)
                {
                    containinedBlocks[i].SetLastTileEntered(containinedBlocks[i].enteredTile.GetRightTile());
                   //

                }
                else
                {
                    containinedBlocks[i].SetLastTileEntered(owningPlayer.GetClosestTile(containinedBlocks[i].transform.position));
                }
            }
            else
            {
                containinedBlocks[i].SetLastTileEntered(owningPlayer.GetClosestTile(containinedBlocks[i].transform.position));
                Debug.LogWarning("enteredTile check in AIMoveRight was found to be null!  Set to nearest tile instead!");
            }

        }
    }
    public void AIMoveDown()
    {
        
            downArrow();
        

        
    }

    public void StressedMoveDown()
    {
        cannotMoveLeftAndRight = false;
        downArrow();
        cannotMoveLeftAndRight = false;



    }

    void setContainedBlocksEnteredTiles() {
        return;
        /*for (int i = 0; i < containinedBlocks.Count; i++)
        {
            s_Tile proposedTile = owningPlayer.GetClosestTile(containinedBlocks[i].transform.position - new Vector3(0, 0.2f, 0));
            if (proposedTile.GetContents() == s_Tile.Contents.NONE)
            {
                containinedBlocks[i].SetLastTileEntered(proposedTile);
            }
            else
            {
                containinedBlocks[i].SetLastTileEntered(proposedTile.FindFreeTileInColumn());
            }
        }*/
    }
}





















