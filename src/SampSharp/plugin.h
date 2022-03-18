// SampSharp
// Copyright 2018 Tim Potze
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#pragma once

#include <string>
#include <filesystem>
#include "ConfigReader.h"
#include "gmhost.h"
#include "liblocator.h"

class plugin
{
public:
    plugin();
    ~plugin();

    gmhost *host() const;
    bool start();
    bool config_validate();
    bool is_config_valid() const;
    
private:
    void config(const std::string &name, std::string &value) const;

    gmhost* host_;
    liblocator locator_;
    bool configvalid_;
    ConfigReader config_;
};
