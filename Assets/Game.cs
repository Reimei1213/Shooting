using Sequence = System.Collections.IEnumerator;
using System;
using System.Collections.Generic;

public sealed class Game : GameBase
{
    // 変数の宣言
    private int gameScene = 0;
    private int interval = 0;
    private int width = 720;
    private int height = 1280;
    private int time = 0;
    private int defeat = 0;  //撃破数
    private int skillbar = 0;
    int ship_x = 358;
    int ship_y = 900;
    
    //弾の情報
    private int[] bullet_x = new int[0];
    private int[] bullet_y = new int[0];
    bool[] bullet_hit=new Boolean[0];
    private int bullet_idx = 0;

    //敵の情報
    private int[] enemy_x = new int[0];
    private int[] enemy_y = new int[0];
    private int[] enemy_life = new int[0];
    private int enemy_idx = 0;
    
    //爆発の情報
    private int[] explo_x = new int[0];
    private int[] explo_y = new int[0];
    private int[] explo_r = new int[0];
    private int explo_idx = 0;
    
    //星の情報
    private int[] stars_x = new int[30];
    private int[] stars_y = new int[30];

    public override void InitGame()
    {
        // キャンバスの大きさを設定します
        gc.SetResolution(width, height);
        CreateStars();
    }
    
    public override void UpdateGame()
    {
        if (gameScene == 0)
            if (gc.GetPointerFrameCount(0) == 1)
                gameScene++;

        if (gameScene == 1)
            Touch();
        
        if(gameScene==2 & interval>=60)
            if (gc.GetPointerFrameCount(0) == 1)
            {
                gameScene = 0;
                Reset();
            }
        if(gameScene==2)
            interval++;
    }
            
    
    public override void DrawGame()
    {
        gc.PlaySound(0,true);
        
        if (gameScene == 0)
        {
            Clear();
            DrawStars();
            gc.DrawImage(8, 0, 0);
            //gc.SetFontSize(60);
            //gc.SetColor(255, 255, 255);
            //gc.DrawString("Shooting Game", 100, 600);
        }
        if (gameScene == 1)
        {
            // 画面を白で塗りつぶします
            //gc.ClearScreen();
            Clear();
            DrawStars();
            gc.SetFontSize(36);

            DrawShip();
            DrawBullet();

            BulletCollision();
            PlayerCollision();
            DrawExplotion();
            
            
            //敵の描画タイミング
            time++;
            if (time % 120 == 0)
                AddEnemy();
            DrawEnemy();
            DrawSkillbar();
            gc.SetColor(255, 255, 255);
            gc.SetFontSize(48);
            gc.DrawString("Score: " + defeat, 500, 0);
            
            //コントローラーを表示
            DrawController();
        }

        if (gameScene == 2)
        {
            Clear();
            DrawStars();
            gc.SetFontSize(80);
            gc.SetColor(255, 255, 255);
            gc.DrawString("GameOver", 100, 400);
            gc.SetFontSize(60);
            gc.DrawString("Score: " + defeat, 100, 700);
        }
    }

    void Reset()
    {
        time = 0;
        defeat = 0;  //撃破数
        skillbar = 0;
        ship_x = 358;
        ship_y = 900;
        
        //弾の情報
        bullet_x = new int[0];
        bullet_y = new int[0];
        bullet_hit=new Boolean[0];
        bullet_idx = 0;

        //敵の情報
        enemy_x = new int[0];
        enemy_y = new int[0];
        enemy_life = new int[0];
        enemy_idx = 0;
        
        //爆発の情報
        explo_x = new int[0]; 
        explo_y = new int[0];
        explo_r = new int[0];
        explo_idx = 0;
    }

    void DrawController()
    {
        gc.SetColor(255, 255, 255);
        gc.FillRect(0, 1040, 720, 240);
        gc.DrawImage(3, 240, 1040);
        gc.DrawImage(4, 0, 1040);
        gc.DrawImage(5, 120, 1040);
        gc.DrawImage(6, 480, 1040);
        gc.DrawImage(7, 480, 1160);
    }

    void Clear()  //背景を黒く描画
    {
        gc.SetColor(0, 0, 0);
        gc.FillRect(0, 0, 720, 1280);
        gc.SetColor(255, 255, 255);
        //gc.DrawLine(1, 0, 1, 1280);
        //gc.DrawLine(720, 0, 720, 1280);
    }

    void DrawShip()  //プレイヤーを描画
    {
        gc.DrawImage(1, ship_x, ship_y);
    }

    void Touch()  //画面をタッチ
    {
        if (gc.GetPointerFrameCount(0) == 1)
        {
            /*
            ship_x = gc.GetPointerX(0);
            if (ship_x < 0)
                ship_x = 0;
            if (ship_x > 720 - 44)
                ship_x = 720 - 44;
            
            ship_y = gc.GetPointerY(0);
            if (ship_y < 0)
                ship_y = 0;
            if (ship_y > 1280 - 66 - 240)
                ship_y = 1280 - 66 - 240;
                */
            CheckController();
            ShootBullet();
        }
    }

    void CheckController()
    {
        int speed = 25;
        int x = gc.GetPointerX(0);
        int y = gc.GetPointerY(0);
        gc.SetFontSize(100);
        
        if (240 <= x & x < 480 & 1040 <= y & y < 1280)  //スキルボタン
        {
            if (skillbar >= 10)
            {
                skillbar = 0;
                for (int i = 0; i < enemy_idx; i++)
                {
                    enemy_life[i] = 0;
                    AddExplotion(enemy_x[i], enemy_y[i]);
                    defeat++;
                }
            }
                   
        }

        if (0 <= x & x < 120 & 1040 <= y & y < 1280)  //左ボタン
        {
            ship_x -= speed;
            if (ship_x < 0)
                ship_x = 0;
        }
        if (120 <= x & x < 240 & 1040 <= y & y < 1280)  //右ボタン
        {
            ship_x += speed;
            if (ship_x > 720 - 44)
                ship_x = 720 - 44;
        }
        if (480 <= x & x < 720 & 1040 <= y & y < 1160)  //上ボタン
        {
            ship_y -= speed;
            //ship_y = 0;
            if (ship_y < 0)
                ship_y = 0;
        }
        if (480 <= x & x < 720 & 1160 <= y & y < 1280)  //下ボタン
        {
            ship_y += speed;
            if (ship_y > 1280 - 66 - 240)
                ship_y = 1280 - 66 - 240;
        }
            
    }

    void ShootBullet()   //弾を管理
    {
        bullet_idx++;
        Array.Resize(ref bullet_x, bullet_idx);
        Array.Resize(ref bullet_y, bullet_idx);
        Array.Resize(ref bullet_hit, bullet_idx);
        bullet_x[bullet_idx-1] = ship_x+22;
        bullet_y[bullet_idx-1] = ship_y;
        bullet_hit[bullet_idx - 1] = false;
    }

    void DrawBullet()  //弾を描画
    {
        int bullet_speed = 10;
        gc.SetColor(0, 255, 255);
        for (int i = 0; i < bullet_idx; i++)
        {
            gc.FillCircle(bullet_x[i], bullet_y[i], 10);
            bullet_y[i] -= bullet_speed;
            
            if (bullet_y[i] < 0 | bullet_hit[i] == true)
                ArrayRemove(i, "bullet");
        }
    }

    void AddEnemy()  //敵の追加
    {
        int spone = gc.Random(0, 720 - 44);
        enemy_idx++;
        Array.Resize(ref enemy_x, enemy_idx);
        Array.Resize(ref enemy_y, enemy_idx);
        Array.Resize(ref enemy_life, enemy_idx);
        enemy_x[enemy_idx - 1] = spone;
        enemy_y[enemy_idx - 1] = 0;
        enemy_life[enemy_idx - 1] = 3;
    }

    void DrawEnemy()  //敵の描画
    {
        int enemy_speed = 2;
        for (int i = 0; i < enemy_idx; i++)
        {
            gc.DrawImage(2, enemy_x[i], enemy_y[i]);
            enemy_y[i] += enemy_speed;
            
            if (enemy_y[i] > 1040 | enemy_life[i] == 0)
                ArrayRemove(i, "enemy");
        }

    }
    

    void ArrayRemove(int i, string mode)  //いらない要素の削除
    {
        if (mode == "bullet")
        {
            bullet_x[i] = bullet_x[bullet_idx - 1];
            bullet_y[i] = bullet_y[bullet_idx - 1];
            bullet_hit[i] = bullet_hit[bullet_idx - 1];
            bullet_idx--;
        }

        if (mode == "enemy")
        {
            enemy_x[i] = enemy_x[enemy_idx - 1];
            enemy_y[i] = enemy_y[enemy_idx - 1];
            enemy_life[i] = enemy_life[enemy_idx - 1];
            enemy_idx--;
        }

        if (mode == "explotion")
        {
            explo_x[i] = explo_x[explo_idx - 1];
            explo_y[i] = explo_y[explo_idx - 1];
            explo_r[i] = explo_r[explo_idx - 1];
            explo_idx--;
        }
    }

    void BulletCollision() //弾の当たり判定
    {
        for (int i = 0; i < enemy_idx; i++)
        {
            for (int j = 0; j < bullet_idx; j++)
            {
                if (bullet_x[j] > enemy_x[i] & bullet_x[j] < enemy_x[i] + 44 &
                    bullet_y[j] > enemy_y[i] & bullet_y[j] < enemy_y[i] + 66)
                {
                    enemy_life[i]--;
                    bullet_hit[j] = true;
                    if (enemy_life[i] == 0)
                    {
                        AddExplotion(enemy_x[i], enemy_y[i]);
                        defeat++;
                        if (skillbar <= 10)
                            skillbar++;
                    }
                }
            }
        }
    }

    void PlayerCollision()
    {
        for (int i = 0; i < enemy_idx; i++)
        {
            if(gc.CheckHitImage(1, ship_x, ship_y, 2, enemy_x[i], enemy_y[i]) == true)
                gameScene = 2;
        }
    }

    void DrawExplotion()
    {
        int ex_speed = 2;
        float x = 0;
        float y = 0;
            
        for (int i = 0; i < explo_idx; i++)
        {
            explo_r[i] += ex_speed;
            gc.SetColor(255, 255, 0, 120);
            for (int angle = 0; angle <= 360; angle += 20)
            {
                x = explo_x[i] + explo_r[i] * gc.Cos((float)angle);
                y = explo_y[i] + explo_r[i] * gc.Sin((float)angle);
                gc.FillCircle((int)x, (int)y, 8);
            }

            if (explo_r[i] > 80)
                ArrayRemove(i, "explotion");
        }
    }

    void AddExplotion(int x, int y)
    {
        int ex_x = x + 22;
        int ex_y = y + 33;
            
        explo_idx++;
        Array.Resize(ref explo_x, explo_idx);
        Array.Resize(ref explo_y, explo_idx);
        Array.Resize(ref explo_r, explo_idx);
        explo_x[explo_idx - 1] = ex_x;
        explo_y[explo_idx - 1] = ex_y;
        explo_r[explo_idx - 1] = 0;
    }

    void CreateStars()
    {
        for (int i = 0; i < 30; i++)
        {
            stars_x[i] = gc.Random(0, width);
            stars_y[i] = gc.Random(0, height);
        }
    }

    void DrawStars()
    {
        int stars_speed = 1;
        gc.SetColor(255, 255, 255);
        for (int i = 0; i < 30; i++)
        {
            gc.FillCircle(stars_x[i], stars_y[i], 2);
            stars_y[i] += stars_speed;
            if (stars_y[i] > height)
                stars_y[i] = 0;
        }
    }

    void DrawSkillbar()
    {
        gc.SetColor(128, 128, 128);
        gc.FillRect(0, 0, width*2/3, 50);
        gc.SetColor(255, 255, 0);
        gc.FillRect(0, 0, width*2/3*skillbar/10, 50);
    }
}
