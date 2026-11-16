shader_type canvas_item;

// 暴露溶解度参数：1为完全不溶解，0为完全溶解
uniform float dissolve_value : hint_range(0.0, 1.0) = 1.0;

// 噪声图，用于决定溶解的形状
uniform sampler2D noise_texture : repeat_enable;

// 溶解边缘的颜色（焦边效果），默认设置成亮橙色
uniform vec4 edge_color : source_color = vec4(1.0, 0.5, 0.0, 1.0);
// 边缘发光的宽度
uniform float edge_width : hint_range(0.0, 1.0) = 0.05;

void fragment() {
    // 获取原图颜色
    vec4 sprite_color = texture(TEXTURE, UV);
    
    // 获取噪声图的红色通道作为溶解阈值
    float noise = texture(noise_texture, UV).r;
    
    // 核心逻辑：
    // 如果噪声值大于溶解度，则丢弃该像素（实现溶解）
    // 当 dissolve_value = 1.0 时，noise > 1.0 几乎不可能，所以不溶解
    // 当 dissolve_value = 0.0 时，noise > 0.0 基本都成立，所以全部丢弃
    if (noise > dissolve_value) {
        discard;
    }
    
    // 边缘发光逻辑：如果噪声值在溶解边界附近
    // 并且确保溶解值不是 0 或 1（防止满屏发光或不发光）
    float edge_threshold = dissolve_value - edge_width;
    if (noise > edge_threshold && dissolve_value > 0.0 && dissolve_value < 1.0) {
        COLOR = edge_color;
    } else {
        COLOR = sprite_color;
    }
}