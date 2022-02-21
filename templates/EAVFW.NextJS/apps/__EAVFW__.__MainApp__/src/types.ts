
export type Task = {
    name: string;
    registrationid: string;
    tasktypeid: string;
    tasktype: TaskDefinition;
    time?: number;
};

export type TaskDefinition = {
    id: string;
    taskname: string;
    parentdefinitionid?: string;
    visiblefortaskcreation?: boolean;
    parentdefinition?: TaskDefinition;
};
