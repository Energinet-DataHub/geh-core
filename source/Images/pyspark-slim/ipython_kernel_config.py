# Copyright 2020 Energinet DataHub A/S
#
# Licensed under the Apache License, Version 2.0 (the "License2");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

# Configuration file for ipython-kernel.
# See <https://ipython.readthedocs.io/en/stable/config/options/kernel.html>

# With IPython >= 6.0.0, all outputs to stdout/stderr are captured.
# It is the case for subprocesses and output of compiled libraries like Spark.
# Those logs now both head to notebook logs and in notebooks outputs.
# Logs are particularly verbose with Spark, that is why we turn them off through this flag.
# <https://github.com/jupyter/docker-stacks/issues/1423>

# Attempt to capture and forward low-level output, e.g. produced by Extension libraries.
# Default: True
# type:ignore
c.IPKernelApp.capture_fd_output = False  # noqa: F821
