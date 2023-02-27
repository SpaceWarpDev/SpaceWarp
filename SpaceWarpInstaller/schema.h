#ifndef __SCHEMA_H
#define __SCHEMA_H

#include <vector>
#include <string>

using namespace std;

class GitRepo {
    public:
        int id;
        string url;
        string name;
};

class GitRef {
    public:
        string ref;
        string sha;
        GitRepo repo;
};

class PullRequest {
    public:
        string url;
        int id;
        int number;
        GitRef head;
        GitRef base;
};

class WorkflowRun {
    public:
        int id;
        string name;
        string node_id;
        string head_branch;
        string head_sha;
        string path;
        string display_title;
        int run_number;
        string event;
        string status;
        string conclusion;
        int workflow_id;
        int check_suite_id;
        string check_suite_node_id;
        string url;
        string html_utl;
        vector<PullRequest> pull_requests;
        string created_at;
        string updated_at;
};

class WorkflowRunsResponse {
    public:
        int total_count;
        vector<WorkflowRun> workflow_runs;
};

#endif
