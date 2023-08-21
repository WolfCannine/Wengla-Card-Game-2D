using System.Collections;
using UnityEngine;

public class TouchManager : MonoBehaviour
{
    #region Variables
    public static TouchManager tm;
    public bool gameOver;
    public GameController controller;
    public GameObject selectedCard;
    public Cell oldCell;
    public LayerMask cardLayer;
    public LayerMask cellLayer;
    private Camera mainCamera;
    private RaycastHit2D rayHit;
    private RaycastHit2D hitCell;
    private Color golden;
    private bool inAnimation;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        tm = this;
        mainCamera = Camera.main;
        golden = new Color(1f, 0.8431373f, 0f);
    }

    private void Update()
    {
        if (inAnimation) { return; }
        if (Input.GetMouseButtonUp(0) && selectedCard == null && !gameOver)
        {
            rayHit = Cast2DRay;
            if (rayHit.collider && rayHit.collider.CompareTag("Card") && rayHit.collider.GetComponent<Card>().pickable)
            {
                selectedCard = rayHit.collider.gameObject;
                SpriteRenderer renderer = selectedCard.GetComponent<SpriteRenderer>();
                renderer.color = golden;
                GameplayUI.gUI.SetSellectedCard(renderer.sprite);
            }
            else if (rayHit.collider && rayHit.collider.CompareTag("Pile"))
            {

            }
        }
        else if (Input.GetMouseButtonUp(0) && selectedCard != null && !gameOver)
        {
            rayHit = Cast2DRay;
            hitCell = Cast2DRayForCell;
            if (hitCell.collider && hitCell.collider.CompareTag("Cell") && !hitCell.collider.GetComponent<Cell>().IsOccupide)
            {

            }
            if (rayHit.collider && rayHit.collider.CompareTag("Card") && selectedCard != rayHit.collider.gameObject
                 && rayHit.collider.GetComponent<Card>().pickable)
            {
                GameObject newCard = rayHit.collider.gameObject;
                GameObject oldCard = selectedCard;
                TransformSet transformSetA = new(newCard.transform.position, newCard.transform.rotation, newCard.transform.localScale);
                TransformSet transformSetB = new(oldCard.transform.position, oldCard.transform.rotation, oldCard.transform.localScale);
                selectedCard.GetComponent<SpriteRenderer>().color = Color.white;
                _ = StartCoroutine(MoveRoutine(transformSetB, transformSetA, newCard));
            }
            else if (rayHit.collider && rayHit.collider.CompareTag("Card") && selectedCard == rayHit.collider.gameObject)
            {
                selectedCard.GetComponent<SpriteRenderer>().color = Color.white;
                selectedCard = null;
                GameplayUI.gUI.SetSellectedCard();
            }
        }
    }
    #endregion

    #region Private Methods
    private RaycastHit2D Cast2DRay => Physics2D.Raycast(mainCamera.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 90f, cardLayer);

    private RaycastHit2D Cast2DRayForCell => Physics2D.Raycast(mainCamera.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 90f, cellLayer);
    #endregion

    #region Animation Routine
    private const float DURATION = 0.5f;

    private IEnumerator MoveRoutine(TransformSet transformSetA, TransformSet transformSetB, GameObject newCard)
    {
        inAnimation = true;
        float elapsedTime = 0;
        float progress = 0;
        while (progress <= 1)
        {
            selectedCard.transform.SetPositionAndRotation(Vector3.Lerp(transformSetA.position, transformSetB.position, progress),
                Quaternion.Slerp(transformSetA.rotation, transformSetB.rotation, progress));
            selectedCard.transform.localScale = Vector3.Lerp(transformSetA.scale, transformSetB.scale, progress);

            newCard.transform.SetPositionAndRotation(Vector3.Lerp(transformSetB.position, transformSetA.position, progress),
                Quaternion.Slerp(transformSetB.rotation, transformSetA.rotation, progress));
            newCard.transform.localScale = Vector3.Lerp(transformSetB.scale, transformSetA.scale, progress);

            elapsedTime += Time.deltaTime;
            progress = elapsedTime / DURATION;
            yield return null;
        }
        selectedCard.transform.SetPositionAndRotation(transformSetB.position, transformSetB.rotation);
        selectedCard.transform.localScale = transformSetB.scale;
        newCard.transform.SetPositionAndRotation(transformSetA.position, transformSetA.rotation);
        newCard.transform.localScale = transformSetA.scale;
        if (selectedCard.transform.localScale.x <= 0.9f)
        {
            GameController.gc.players[0].exposeCardID = selectedCard.GetComponent<Card>().cardID;
        }
        selectedCard = null;
        GameplayUI.gUI.SetSellectedCard();
        inAnimation = false;
    }
    #endregion
}
