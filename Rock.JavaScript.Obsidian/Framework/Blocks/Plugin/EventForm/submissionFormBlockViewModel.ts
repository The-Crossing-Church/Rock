import { DefinedValue, ContentChannelItem, ContentChannelItemAssociation } from "../../../ViewModels"

export type SubmissionFormBlockViewModel = {
    request: ContentChannelItem;
    originalRequest: ContentChannelItem;
    events: ContentChannelItem[];
    existing: ContentChannelItem[];
    existingDetails: ContentChannelItemAssociation[];
    isSuperUser: boolean;
    isEventAdmin: boolean;
    isRoomAdmin: boolean;
    permissions: string[];
    locations: DefinedValue[];
    ministries: DefinedValue[];
    budgetLines: DefinedValue[];
    adminDashboardURL: string;
    userDashboardURL: string;
};