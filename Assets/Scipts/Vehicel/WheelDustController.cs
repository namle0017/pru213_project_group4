using UnityEngine;

public class WheelDustController : MonoBehaviour
{
    private ParticleSystem dustParticles;
    private Rigidbody2D rb; // Rigidbody2D của bánh xe
    
    [Header("Settings")]
    [Tooltip("Tốc độ xoay tối thiểu của bánh xe để sinh ra bụi")]
    public float minSpeedForDust = 50f;

    [Tooltip("Hệ số nhân số lượng bụi sinh ra theo tốc độ quay")]
    public float speedEmissionMultiplier = 0.5f;

    // Các màu bụi tương ứng với từng Map
    [Header("Dust Colors by Map Tag")]
    public Color dirtColor = new Color(0.6f, 0.4f, 0.2f, 0.5f); // Nâu đất
    public Color snowColor = new Color(1f, 1f, 1f, 0.8f);       // Trắng tuyết
    public Color marsColor = new Color(0.8f, 0.3f, 0.1f, 0.6f); // Đỏ sao Hỏa
    public Color moonColor = new Color(0.5f, 0.5f, 0.5f, 0.5f); // Xám mặt trăng
    
    private bool isTouchingGround = false;

    private void Start()
    {
        // Tự động tìm ParticleSystem nằm bên trong bánh xe này (kể cả nó là object con)
        dustParticles = GetComponentInChildren<ParticleSystem>();
        
        // Lấy Rigidbody2D của chính bánh xe này để đo tốc độ quay
        rb = GetComponent<Rigidbody2D>();
        
        if (dustParticles != null)
        {
            dustParticles.Stop(); // Đảm bảo lúc mới vào game bụi không bay lung tung
            Debug.Log("Đã tìm thấy ParticleSystem trên bánh xe: " + gameObject.name);
        }
        else 
        {
            Debug.LogError("KHÔNG TÌM THẤY Particle System trên " + gameObject.name + "! Hãy kiểm tra lại!");
        }
    }

    private void Update()
    {
        if (dustParticles == null || rb == null) return;

        // Tính tốc độ quay hiện tại của bánh xe (dùng trị tuyệt đối để tiến hay lùi đều ra số dương)
        float currentWheelSpeed = Mathf.Abs(rb.angularVelocity);

        // Bánh xe phải CHẠM ĐẤT và QUAY ĐỦ NHANH thì mới phun bụi
        if (isTouchingGround && currentWheelSpeed > minSpeedForDust)
        {
            if (!dustParticles.isPlaying)
            {
                dustParticles.Play();
                Debug.Log(gameObject.name + " BẮT ĐẦU PHUN BỤI! Tốc độ: " + currentWheelSpeed);
            }
            // CẬP NHẬT ĐỘNG: Lấy module Emission của Particle và gán tốc độ sinh bụi tỉ lệ thuận với tốc độ quay bánh xe
            var emission = dustParticles.emission;
            emission.rateOverTime = currentWheelSpeed * speedEmissionMultiplier;
        }
        else
        {
            if (dustParticles.isPlaying)
            {
                dustParticles.Stop();
                Debug.Log(gameObject.name + " NGỪNG PHUN BỤI. isTouchingGround: " + isTouchingGround + ", Tốc độ: " + currentWheelSpeed);
            }
        }
    }

    // Hàm này chạy liên tục khi bánh xe đang cọ xát với vật khác (Mặt đất)
    private void OnCollisionStay2D(Collision2D collision)
    {
        isTouchingGround = true;

        if (dustParticles == null) return;

        var mainModule = dustParticles.main;

        // ĐỌC TAG của mặt đất và ĐỔI MÀU BỤI
        if (collision.gameObject.CompareTag("DirtGround"))
        {
            mainModule.startColor = dirtColor;
        }
        else if (collision.gameObject.CompareTag("SnowGround"))
        {
            mainModule.startColor = snowColor;
        }
        else if (collision.gameObject.CompareTag("MarsGround"))
        {
            mainModule.startColor = marsColor;
        }
        else if (collision.gameObject.CompareTag("MoonGround"))
        {
            mainModule.startColor = moonColor;
        }
        else 
        {
            // Nếu mặt đất chưa được gắn Tag đúng, nó sẽ giữ nguyên màu mặc định.
            // Bật dòng này lên để xem mặt đất đang có Tag là gì
            // Debug.Log("Bánh xe đang chạm vào vật có Tag là: " + collision.gameObject.tag);
        }
    }

    // Hàm này chạy khi bánh xe nảy lên không trung (rời khỏi mặt đất)
    private void OnCollisionExit2D(Collision2D collision)
    {
        isTouchingGround = false;
        
        if (dustParticles != null && dustParticles.isPlaying)
        {
            dustParticles.Stop();
        }
    }
}
