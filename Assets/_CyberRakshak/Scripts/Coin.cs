using UnityEngine;

namespace CyberRakshak.Runtime
{
public class Coin : MonoBehaviour
{
    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 120f;

    [Header("Shine")]
    [SerializeField] private bool useEmission = true;
    [SerializeField] private Color emissionColor = new Color(1f, 0.65f, 0.05f);
    [SerializeField] private float emissionIntensity = 1.5f;

    private CoinManager coinManager;
    private bool collected = false;

    private Material coinMaterial;

    public void Initialize(CoinManager manager)
    {
        coinManager = manager;
    }

    private void Start()
    {
        SetupShine();
    }

    private void Update()
    {
        // Rotate the coin around its Y axis
        transform.Rotate(
            0f,
            rotationSpeed * Time.deltaTime,
            0f,
            Space.Self
        );
    }

    private void SetupShine()
    {
        if (!useEmission)
            return;

        Renderer renderer = GetComponentInChildren<Renderer>();

        if (renderer == null)
        {
            Debug.LogWarning(
                "Coin: No Renderer found on coin prefab."
            );
            return;
        }

        coinMaterial = renderer.material;

        if (coinMaterial.HasProperty("_Metallic"))
        {
            coinMaterial.SetFloat("_Metallic", 0.8f);
        }

        if (coinMaterial.HasProperty("_Smoothness"))
        {
            coinMaterial.SetFloat("_Smoothness", 0.9f);
        }

        if (coinMaterial.HasProperty("_EmissionColor"))
        {
            Color emission =
                emissionColor * emissionIntensity;

            coinMaterial.SetColor(
                "_EmissionColor",
                emission
            );

            coinMaterial.EnableKeyword("_EMISSION");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collected || coinManager == null)
            return;

        if (!other.CompareTag("Player") && !other.transform.root.CompareTag("Player"))
            return;

        collected = true;

        if (coinManager != null)
        {
            coinManager.CollectCoin(this);
        }
    }

    private void OnDestroy()
    {
        if (coinMaterial != null) Destroy(coinMaterial);
    }
}
}
