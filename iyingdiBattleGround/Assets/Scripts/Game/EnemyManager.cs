using System;
using System.Collections.Generic;

public class EnemyManager 
{
    static readonly Random random = new Random(unchecked((int)DateTime.Now.Ticks));
    public static List<Enemy> enemies = new List<Enemy>();
    public static List<Enemy> lowEnemies = new List<Enemy>() { new OldMurkEye(), new SilverbackPatriarch()};
    public static List<Enemy> midEnemies = new List<Enemy>() { new Boom(), new Millificent(), new Jaraxxus(), new Trion() };

    public static void CreateEnemy()
    {
        lowEnemies = new List<Enemy>() { new OldMurkEye(), new SilverbackPatriarch() };
        midEnemies = new List<Enemy>() { new Boom(), new Millificent(), new Jaraxxus(), new Trion() };

        enemies = new List<Enemy>
        {
            lowEnemies.GetOneRandomly()
        };
        List<Enemy> tmpEnemies = midEnemies;
        tmpEnemies.Shuffle();
        foreach (var item in tmpEnemies)
        {
            if (enemies.Count == 3)
            {
                break;
            }
            enemies.Add(item);
        }
        enemies.Add(new Curator());
        foreach (var enemy in enemies)
        {
            enemy.InitCardPile(CardPile.GetFullCardPile());
        }
    }

    public static Enemy GetEnemyByLevel(int level)
    {
        if (enemies.Count == 0)
        {
            CreateEnemy();
        }
        return enemies[level - 1];
    }

    public static Player GetEnemy()
    {
        if (enemies.Count == 0)
        {
            CreateEnemy();
        }
        return enemies.GetOne().GetPlayerForBattle();
    }
}
