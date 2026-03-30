using UnityEngine;

[DisallowMultipleComponent]
public class PresentPastSceneManager : MonoBehaviour
{
    public enum WorldState
    {
        Present,
        Past
    }

    [Header("Roots")]
    [SerializeField] private GameObject presentRoot;
    [SerializeField] private GameObject pastRoot;

    [Header("Switching")]
    [SerializeField] private KeyCode switchKey = KeyCode.E;
    [SerializeField] private WorldState initialState = WorldState.Present;
    [SerializeField] private bool movePlayerWithRootOffset = true;

    [Header("References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Rigidbody2D playerBody;

    public WorldState CurrentState { get; private set; }

    private void Awake()
    {
        CachePlayerReferences();
        SetState(initialState, false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(switchKey))
        {
            Toggle();
        }
    }

    [ContextMenu("Switch To Present")]
    public void SwitchToPresent()
    {
        SetState(WorldState.Present, true);
    }

    [ContextMenu("Switch To Past")]
    public void SwitchToPast()
    {
        SetState(WorldState.Past, true);
    }

    [ContextMenu("Toggle World")]
    public void Toggle()
    {
        var next = CurrentState == WorldState.Present ? WorldState.Past : WorldState.Present;
        SetState(next, true);
    }

    private void SetState(WorldState nextState, bool movePlayer)
    {
        if (presentRoot == null || pastRoot == null)
        {
            return;
        }

        CachePlayerReferences();

        var currentRoot = CurrentState == WorldState.Present ? presentRoot.transform : pastRoot.transform;
        var targetRoot = nextState == WorldState.Present ? presentRoot.transform : pastRoot.transform;

        if (movePlayer && movePlayerWithRootOffset && playerTransform != null && currentRoot != targetRoot)
        {
            var position = playerTransform.position + (targetRoot.position - currentRoot.position);
            playerTransform.position = position;

            if (playerBody != null)
            {
                playerBody.position = position;
            }
        }

        presentRoot.SetActive(nextState == WorldState.Present);
        pastRoot.SetActive(nextState == WorldState.Past);
        CurrentState = nextState;
    }

    private void CachePlayerReferences()
    {
        if (playerTransform == null)
        {
            var player = FindObjectOfType<PlayerController2D>();
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        if (playerBody == null && playerTransform != null)
        {
            playerBody = playerTransform.GetComponent<Rigidbody2D>();
        }
    }
}
