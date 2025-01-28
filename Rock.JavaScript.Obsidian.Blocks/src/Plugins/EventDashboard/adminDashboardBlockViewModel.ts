import { ContentChannelItemBag } from "@Obsidian/ViewModels/Entities/contentChannelItemBag"
import { DefinedValueBag } from "@Obsidian/ViewModels/Entities/definedValueBag"
import { AttributeBag } from "@Obsidian/ViewModels/Entities/attributeBag"
import { PersonBag } from "@Obsidian/ViewModels/Entities/personBag"

export type AdminDashboardBlockViewModel = {
    events: ContentChannelItemBag[];
    submittedEvents: ContentChannelItemBag[];
    changedEvents: ContentChannelItemBag[];
    inprogressEvents: ContentChannelItemBag[];
    comments: ContentChannelItemBag[];
    isEventAdmin: boolean;
    isRoomAdmin: boolean;
    locations: DefinedValueBag[];
    ministries: DefinedValueBag[];
    budgetLines: DefinedValueBag[];
    drinks: DefinedValueBag[];
    requestStatus: AttributeBag;
    requestType: AttributeBag;
    workflowURL: string;
    defaultStatuses: string[];
    users: PersonBag[];
};