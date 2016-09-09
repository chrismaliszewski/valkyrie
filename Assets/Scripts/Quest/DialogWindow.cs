﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Events;

// Class for creation of a dialog window with buttons and handling button press
public class DialogWindow {
    // The even that raises this dialog
    public EventManager.Event eventData;
    public List<Round.Hero> heroList;

    public DialogWindow(EventManager.Event e)
    {
        eventData = e;
        heroList = new List<Round.Hero>();
        Game game = Game.Get();

        if (!eventData.qEvent.heroListName.Equals(""))
        {
            if (!game.quest.heroSelection.ContainsKey(eventData.qEvent.heroListName))
            {
                Debug.Log("Warning: Hero selection in event: " + eventData.qEvent.name + " from event " + eventData.qEvent.heroListName + " with no data.");
            }
            else
            {
                foreach (Round.Hero h in game.quest.heroSelection[eventData.qEvent.heroListName])
                {
                    h.selected = true;
                }
                game.heroCanvas.UpdateStatus();
            }
        }

        CreateWindow();
    }

    public void CreateWindow()
    {
        DialogBox db = new DialogBox(new Vector2(10, 0.5f), new Vector2(UIScaler.GetWidthUnits() - 20, 8), eventData.GetText());
        db.AddBorder();

        // Do we have a cancel button?
        if (eventData.qEvent.cancelable)
        {
            new TextButton(new Vector2(11, 9f), new Vector2(8f, 2), "Cancel", delegate { onCancel(); });
        }

        // If there isn't a fail event we have a confirm button
        if (eventData.ConfirmPresent())
        {
            new TextButton(new Vector2(UIScaler.GetWidthUnits() - 19, 9f), new Vector2(8f, 2), eventData.GetPass(), delegate { onConfirm(); }, eventData.GetPassColor());
            if (eventData.FailPresent())
            {
                new TextButton(new Vector2(UIScaler.GetWidthUnits() - 19, 11.5f), new Vector2(8f, 2), eventData.GetFail(), delegate { onFail(); }, eventData.GetFailColor());
            }
        }
    }

    // Pass and confirm are the same
    public void onPass()
    {
        onConfirm();
    }

    // Cancel cleans up
    public void onCancel()
    {
        destroy();
        Game.Get().quest.eManager.TriggerEvent();
    }

    public void onFail()
    {
        if (!checkHeroes()) return;

        Game game = Game.Get();
        // Destroy this dialog to close
        destroy();
        // Trigger failure event
        // Trigger next event
        foreach (string e in eventData.qEvent.failEvent)
        {
            if (!game.quest.eManager.events[e].Disabled())
            {
                game.quest.eManager.QueueEvent(e);
                return;
            }
        }

        // Nothing more to queue from this event, but there might be other stuff in the queue, so trigger that
        game.quest.eManager.TriggerEvent();
    }

    public bool checkHeroes()
    {
        Game game = Game.Get();

        heroList = new List<Round.Hero>();

        foreach (Round.Hero h in game.round.heroes)
        {
            if (h.selected)
            {
                heroList.Add(h);
            }
        }

        if (eventData.qEvent.maxHeroes < heroList.Count && eventData.qEvent.maxHeroes != 0) return false;
        if (eventData.qEvent.minHeroes > heroList.Count) return false;

        foreach (Round.Hero h in game.round.heroes)
        {
            h.selected = false;
        }

        if (game.quest.heroSelection.ContainsKey(eventData.qEvent.name))
        {
            game.quest.heroSelection.Remove(eventData.qEvent.name);
        }
        game.quest.heroSelection.Add(eventData.qEvent.name, heroList);

        game.heroCanvas.UpdateStatus();

        return true;
    }


    public void onConfirm()
    {
        if (!checkHeroes()) return;

        Game game = Game.Get();

        // Destroy this dialog to close
        destroy();
        // Trigger next event
        foreach (string e in eventData.qEvent.nextEvent)
        {
            if (!game.quest.eManager.events[e].Disabled())
            {
                game.quest.eManager.QueueEvent(e);
                return;
            }
        }
        if (eventData.qEvent.name.IndexOf("EventEnd") == 0)
        {
            Destroyer.MainMenu();
            return;
        }

        // Nothing more to queue from this event, but there might be other stuff in the queue, so trigger that
        game.quest.eManager.TriggerEvent();
    }

    public void destroy()
    {
        // Clean up everything marked as 'dialog'
        Destroyer.Dialog();

        Game.Get().quest.eManager.currentEvent = null;
    }
}
